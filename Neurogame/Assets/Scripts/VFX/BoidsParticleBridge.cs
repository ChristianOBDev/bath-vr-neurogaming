using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// Boids logic layer that drives an existing ParticleSystem's particle positions.
/// Attach to the same GameObject as your birds_fantail ParticleSystem,
/// or assign the ParticleSystem reference in the inspector.
///
/// The particle system handles rendering (animated Synty FBX via mesh renderer mode).
/// This script handles flocking intelligence via SetParticles().
///
/// EEG INTEGRATION (BCIforVFX.cs):
///   Your existing BCIforVFX.cs can call the public setters below,
///   or you can keep driving particle properties (colour, size) via BCIforVFX
///   while this script only overrides position/velocity/rotation.
///
/// SETUP:
///   1. On your birds_fantail particle system, set:
///      - Emission: burst of N at t=0 (match boidCount), rate over time = 0
///      - Start Lifetime: Infinity (9999)
///      - Start Speed: 0
///      - Simulation Space: World
///      - Max Particles: match boidCount
///      - DISABLE: Shape, Velocity over Lifetime, Force over Lifetime,
///        Limit Velocity over Lifetime, Noise (boids replaces all of these)
///      - KEEP ENABLED: Renderer (mesh mode with your Synty FBX),
///        any colour/size modules you want BCIforVFX to still control
///   2. Attach this script. Assign the particle system ref if not on same GO.
///   3. Hit play. Particles will flock.
/// </summary>
public class BoidsParticleBridge : MonoBehaviour
{
    [Header("Target Particle System")]
    [Tooltip("Leave null to auto-detect on this GameObject")]
    [SerializeField] private ParticleSystem targetParticleSystem;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;
    [SerializeField] private float debugRayLength = 1.5f;

    [Header("Flock Settings")]
    [SerializeField] private int boidCount = 200;
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

    [Header("Noise & EEG")]
    [SerializeField] private float noiseStrength = 0.3f;
    [SerializeField] private float scatter = 0f;

    [Header("Rotation")]
    [SerializeField] private Vector3 meshRotationOffset = new Vector3(90f, 0f, 0f);
    [Tooltip("How quickly particles rotate to face their velocity direction")]
    [SerializeField] private float rotationSmoothing = 8f;

    // Boid state
    private NativeArray<float3> positions;
    private NativeArray<float3> velocities;
    private NativeArray<float3> accelerations;

    // Particle system cache
    private ParticleSystem.Particle[] particles;
    private bool initialised = false;

    #region EEG Public API

    public void SetCohesionWeight(float w) => cohesionWeight = Mathf.Clamp(w, 0f, 10f);
    public void SetAlignmentWeight(float w) => alignmentWeight = Mathf.Clamp(w, 0f, 5f);
    public void SetSeparationWeight(float w) => separationWeight = Mathf.Clamp(w, 0f, 5f);
    public void SetSpeed(float s) => speed = Mathf.Clamp(s, 0.5f, maxSpeed);
    public void SetMaxSpeed(float s) => maxSpeed = Mathf.Clamp(s, 1f, 20f);

    public void SetScatter(float s) => scatter = Mathf.Clamp01(s);

    // Read-only accessors so BCIforVFX can query state if needed
    public int BoidCount => boidCount;
    public float CurrentSpeed => speed;
    public float CurrentScatter => scatter;

    #endregion

    private void Awake()
    {
        if (targetParticleSystem == null)
            targetParticleSystem = GetComponent<ParticleSystem>();

        if (targetParticleSystem == null)
        {
            Debug.LogError("[BoidsParticleBridge] No ParticleSystem found. Assign one or attach to same GameObject.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Let the particle system emit first frame, then we take over
        Invoke(nameof(InitialiseBoids), 0.1f);
    }

    private void InitialiseBoids()
    {
        // Ensure particle system has emitted enough particles
        int aliveCount = targetParticleSystem.particleCount;
        if (aliveCount < boidCount)
        {
            // Force-emit the difference
            targetParticleSystem.Emit(boidCount - aliveCount);
            aliveCount = targetParticleSystem.particleCount;
        }

        // Clamp boidCount to what we actually have
        boidCount = Mathf.Min(boidCount, aliveCount);

        // Allocate native arrays
        positions = new NativeArray<float3>(boidCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(boidCount, Allocator.Persistent);
        accelerations = new NativeArray<float3>(boidCount, Allocator.Persistent);
        particles = new ParticleSystem.Particle[aliveCount];

        // Read current particle positions or randomise
        targetParticleSystem.GetParticles(particles);

        var rng = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        float3 centre = (float3)transform.position;

        for (int i = 0; i < boidCount; i++)
        {
            positions[i] = centre + rng.NextFloat3Direction() * rng.NextFloat(0f, spawnRadius);
            velocities[i] = rng.NextFloat3Direction() * speed;
            accelerations[i] = float3.zero;
        }

        initialised = true;
    }

    private void LateUpdate()
    {
        if (!initialised) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- Run boids logic (same Burst job) ---
        var job = new BoidsJob
        {
            positions = positions,
            velocities = velocities,
            accelerations = accelerations,
            perceptionRadius = perceptionRadius,
            avoidanceRadius = avoidanceRadius,
            cohesionWeight = cohesionWeight,
            alignmentWeight = alignmentWeight,
            separationWeight = separationWeight * (1f + scatter * 3f),
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

        // --- Integrate velocities ---
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

        // --- Write positions back to particle system ---
        int aliveCount = targetParticleSystem.GetParticles(particles);

        for (int i = 0; i < boidCount && i < aliveCount; i++)
        {
            particles[i].position = (Vector3)positions[i];

            // Set particle velocity so the renderer knows the direction
            // (useful if you have stretched billboard or mesh alignment)
            particles[i].velocity = (Vector3)velocities[i];

            // Rotation: face velocity direction
            float3 vel = velocities[i];
            float spdSq = math.lengthsq(vel);
            if (spdSq > 0.01f)
            {
                // Convert velocity to euler Y rotation for particle system
                // ParticleSystem rotation3D uses degrees
                float3 dir = math.normalize(vel);
                Quaternion targetQuat = Quaternion.LookRotation(vel) * Quaternion.Euler(meshRotationOffset);
                Quaternion currentQuat = Quaternion.Euler(particles[i].rotation3D);
                Quaternion smoothed = Quaternion.Slerp(currentQuat, targetQuat, dt * rotationSmoothing);
                particles[i].rotation3D = smoothed.eulerAngles;
            }

            // Keep particles alive indefinitely
            particles[i].remainingLifetime = 9999f;
        }

        targetParticleSystem.SetParticles(particles, aliveCount);
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

    private void OnDrawGizmos()
    {
        if (!showDebugRays || !initialised || !Application.isPlaying) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 pos = (Vector3)positions[i];
            Vector3 dir = math.normalize(velocities[i]);
            Gizmos.DrawRay(pos, dir * debugRayLength);
        }
    }

    // =========================================================================
    // Burst job — identical logic to BoidsFlock.cs
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
                        separation -= diff / math.sqrt(distSq);
                    }
                }
            }

            float3 accel = float3.zero;

            if (neighbours > 0)
            {
                cohesion = (cohesion / neighbours) - pos;
                accel += cohesion * cohesionWeight;

                alignment = alignment / neighbours;
                accel += alignment * alignmentWeight;

                accel += separation * separationWeight;
            }

            // Boundary
            accel += BoundaryForce(pos);

            // Noise
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