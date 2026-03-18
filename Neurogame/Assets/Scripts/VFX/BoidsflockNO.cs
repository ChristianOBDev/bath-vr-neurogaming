using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// High-performance boids flocking system for Unity.
/// Replaces particle-system birds with proper flocking behaviour.
/// Uses Unity Jobs + Burst for parallel spatial queries.
/// 
/// USAGE:
///   1. Attach to an empty GameObject.
///   2. Assign a bird mesh + material (GPU instanced).
///   3. Optionally connect EEG parameters via public setters.
///   
/// EEG INTEGRATION (BCIforVFX.cs):
///   - Call SetCohesionWeight(), SetAlignmentWeight(), SetSeparationWeight()
///   - Call SetSpeed() to map e.g. alpha band → flock speed
///   - Call SetScatter() to break formation (maps well to beta/stress)
/// </summary>
public class BoidsFlock : MonoBehaviour
{
    [Header("Flock Settings")]
    [SerializeField] private int boidCount = 200;
    [SerializeField] private Mesh boidMesh;
    [SerializeField] private Material boidMaterial;
    [SerializeField] private float spawnRadius = 10f;

    [Header("Boid Rules")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float perceptionRadius = 5f;
    [SerializeField] private float avoidanceRadius = 1.5f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float boundaryWeight = 2f;

    [Header("Boundary")]
    [SerializeField] private Vector3 boundarySize = new Vector3(30f, 20f, 30f);
    [SerializeField] private float boundaryTurnMargin = 5f;

    [Header("Noise & Variation")]
    [SerializeField] private float noiseStrength = 0.3f;
    [SerializeField] private float scatter = 0f; // EEG-driven: 0 = tight flock, 1 = chaotic

    [Header("Visual")]
    [SerializeField] private float boidScale = 0.15f;
    [SerializeField] private float bankAngle = 30f;

    // Runtime data
    private NativeArray<float3> positions;
    private NativeArray<float3> velocities;
    private NativeArray<float3> accelerations;
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock propertyBlock;

    // GPU instancing batch limit
    private const int BATCH_SIZE = 1023;

    #region EEG Public API

    /// <summary>Set cohesion weight. Maps well to alpha (relaxed = tighter flock).</summary>
    public void SetCohesionWeight(float w) => cohesionWeight = Mathf.Clamp(w, 0f, 5f);

    /// <summary>Set alignment weight.</summary>
    public void SetAlignmentWeight(float w) => alignmentWeight = Mathf.Clamp(w, 0f, 5f);

    /// <summary>Set separation weight.</summary>
    public void SetSeparationWeight(float w) => separationWeight = Mathf.Clamp(w, 0f, 5f);

    /// <summary>Set base speed. Maps well to overall arousal.</summary>
    public void SetSpeed(float s) => speed = Mathf.Clamp(s, 0.5f, maxSpeed);

    /// <summary>Set scatter (0-1). High beta/stress → flock breaks apart.</summary>
    public void SetScatter(float s) => scatter = Mathf.Clamp01(s);

    /// <summary>Set boid count at runtime. Triggers rebuild.</summary>
    public void SetBoidCount(int count)
    {
        if (count == boidCount || count < 1) return;
        Cleanup();
        boidCount = count;
        Initialise();
    }

    #endregion

    private void Awake()
    {
        if (boidMesh == null || boidMaterial == null)
        {
            Debug.LogError("[BoidsFlock] Assign boidMesh and boidMaterial in the inspector.");
            enabled = false;
            return;
        }

        // Material must support instancing
        if (!boidMaterial.enableInstancing)
        {
            Debug.LogWarning("[BoidsFlock] Enabling GPU instancing on material.");
            boidMaterial.enableInstancing = true;
        }

        Initialise();
    }

    private void Initialise()
    {
        positions = new NativeArray<float3>(boidCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(boidCount, Allocator.Persistent);
        accelerations = new NativeArray<float3>(boidCount, Allocator.Persistent);
        matrices = new Matrix4x4[boidCount];
        propertyBlock = new MaterialPropertyBlock();

        var rng = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        float3 centre = ((float3)transform.position);

        for (int i = 0; i < boidCount; i++)
        {
            positions[i] = centre + rng.NextFloat3Direction() * rng.NextFloat(0f, spawnRadius);
            velocities[i] = rng.NextFloat3Direction() * speed;
            accelerations[i] = float3.zero;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- Schedule boids job ---
        var job = new BoidsJob
        {
            positions = positions,
            velocities = velocities,
            accelerations = accelerations,
            perceptionRadius = perceptionRadius,
            avoidanceRadius = avoidanceRadius,
            cohesionWeight = cohesionWeight,
            alignmentWeight = alignmentWeight,
            separationWeight = separationWeight * (1f + scatter * 3f), // scatter boosts separation
            boundaryWeight = boundaryWeight,
            boundaryMin = (float3)transform.position - (float3)boundarySize * 0.5f,
            boundaryMax = (float3)transform.position + (float3)boundarySize * 0.5f,
            boundaryMargin = boundaryTurnMargin,
            noiseStrength = noiseStrength + scatter * 2f,
            time = Time.timeSinceLevelLoad,
            boidCount = boidCount
        };

        var handle = job.Schedule(boidCount, 64);
        handle.Complete();

        // --- Integrate ---
        float currentMaxSpeed = maxSpeed * (1f + scatter * 0.5f);
        for (int i = 0; i < boidCount; i++)
        {
            velocities[i] += accelerations[i] * dt;

            float spd = math.length(velocities[i]);
            if (spd > currentMaxSpeed)
                velocities[i] = math.normalize(velocities[i]) * currentMaxSpeed;
            else if (spd < speed * 0.5f)
                velocities[i] = math.normalize(velocities[i]) * speed * 0.5f;

            positions[i] += velocities[i] * dt;
        }

        // --- Build matrices for GPU instancing ---
        for (int i = 0; i < boidCount; i++)
        {
            float3 vel = velocities[i];
            float spd = math.length(vel);
            float3 forward = spd > 0.001f ? vel / spd : new float3(0, 0, 1);
            float3 up = new float3(0, 1, 0);

            // Bank into turns
            if (spd > 0.1f)
            {
                float3 accel = accelerations[i];
                float3 side = math.cross(forward, up);
                float bankDot = math.dot(accel, side);
                float bankRad = math.radians(bankAngle) * math.clamp(bankDot * 0.5f, -1f, 1f);
                quaternion bankRot = quaternion.AxisAngle(forward, bankRad);
                up = math.rotate(bankRot, up);
            }

            quaternion rot = quaternion.LookRotationSafe(forward, up);
            float3 scl = new float3(boidScale, boidScale, boidScale);

            matrices[i] = float4x4.TRS(positions[i], rot, scl);
        }

        // --- Draw instanced ---
        int remaining = boidCount;
        int offset = 0;
        while (remaining > 0)
        {
            int batch = Mathf.Min(remaining, BATCH_SIZE);
            var slice = new Matrix4x4[batch];
            System.Array.Copy(matrices, offset, slice, 0, batch);
            Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, slice, batch, propertyBlock);
            remaining -= batch;
            offset += batch;
        }
    }

    private void Cleanup()
    {
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
        if (accelerations.IsCreated) accelerations.Dispose();
    }

    private void OnDestroy() => Cleanup();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.15f);
        Gizmos.DrawWireCube(transform.position, boundarySize);
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.1f);
        Gizmos.DrawWireCube(transform.position, boundarySize - Vector3.one * boundaryTurnMargin * 2f);
    }

    // =========================================================================
    // Burst-compiled parallel job for O(n²) neighbour queries
    // For >1000 boids, swap to spatial hashing (see notes at bottom)
    // =========================================================================
    [BurstCompile]
    private struct BoidsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<float3> velocities;
        [WriteOnly] public NativeArray<float3> accelerations;

        public float perceptionRadius;
        public float avoidanceRadius;
        public float cohesionWeight;
        public float alignmentWeight;
        public float separationWeight;
        public float boundaryWeight;
        public float3 boundaryMin;
        public float3 boundaryMax;
        public float boundaryMargin;
        public float noiseStrength;
        public float time;
        public int boidCount;

        public void Execute(int i)
        {
            float3 pos = positions[i];
            float3 vel = velocities[i];

            float3 cohesion = float3.zero;
            float3 alignment = float3.zero;
            float3 separation = float3.zero;
            int neighbours = 0;

            float percSq = perceptionRadius * perceptionRadius;
            float avoidSq = avoidanceRadius * avoidanceRadius;

            for (int j = 0; j < boidCount; j++)
            {
                if (j == i) continue;

                float3 diff = positions[j] - pos;
                float distSq = math.lengthsq(diff);

                if (distSq < percSq && distSq > 0.0001f)
                {
                    cohesion += positions[j];
                    alignment += velocities[j];
                    neighbours++;

                    if (distSq < avoidSq)
                    {
                        separation -= diff / math.sqrt(distSq); // weight by inverse distance
                    }
                }
            }

            float3 accel = float3.zero;

            if (neighbours > 0)
            {
                // Cohesion: steer toward average position
                cohesion = (cohesion / neighbours) - pos;
                accel += cohesion * cohesionWeight;

                // Alignment: steer toward average heading
                alignment = alignment / neighbours;
                accel += alignment * alignmentWeight;

                // Separation: steer away from close neighbours
                accel += separation * separationWeight;
            }

            // Boundary avoidance (soft walls)
            accel += BoundaryForce(pos);

            // Noise (per-boid deterministic using index + time)
            float phase = time * 1.3f + i * 0.7f;
            float3 noise = new float3(
                math.sin(phase * 1.1f + i * 0.37f),
                math.sin(phase * 0.9f + i * 0.53f),
                math.sin(phase * 1.3f + i * 0.71f)
            );
            accel += noise * noiseStrength;

            accelerations[i] = accel;
        }

        private float3 BoundaryForce(float3 pos)
        {
            float3 force = float3.zero;

            // For each axis, apply steering when within margin of boundary
            for (int axis = 0; axis < 3; axis++)
            {
                float lo = boundaryMin[axis] + boundaryMargin;
                float hi = boundaryMax[axis] - boundaryMargin;

                if (pos[axis] < lo)
                {
                    float t = (lo - pos[axis]) / boundaryMargin;
                    force[axis] = boundaryWeight * math.clamp(t, 0f, 1f);
                }
                else if (pos[axis] > hi)
                {
                    float t = (pos[axis] - hi) / boundaryMargin;
                    force[axis] = -boundaryWeight * math.clamp(t, 0f, 1f);
                }
            }

            return force;
        }
    }
}

// =============================================================================
// SPATIAL HASHING UPGRADE NOTES (for >500 boids):
// 
// The O(n²) brute-force neighbour search above is fine for ~200 boids on Quest/PC.
// For larger flocks, replace with NativeMultiHashMap<int, int> spatial grid:
//
//   1. Quantise each position to a cell: int3 cell = (int3)math.floor(pos / cellSize)
//   2. Hash: cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791
//   3. In Execute(), only iterate boids in the 27 adjacent cells.
//   4. Drops from O(n²) to ~O(n) for uniform distributions.
//
// Also consider: Unity DOTS Entities for full ECS if you're scaling to 1000+.
// =============================================================================