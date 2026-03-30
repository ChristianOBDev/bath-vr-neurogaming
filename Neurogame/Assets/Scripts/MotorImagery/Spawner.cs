using System.Collections;
using UnityEngine;
using MotorImagery;

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour
{
    [Header("Debug")]
    public bool spawnerDebugLogging = false;

    [Header("Spawned Object")]
    public GameObject bumperPrefab;

    [Header("Spawn Settings")]
    [Min(0f)]
    public float respawnDelay = 0.3f;
    public Transform bumperParent;

    [Min(1)]
    public int bumpersPerBurst = 1;

    [Header("Placement Rules")]
    public float minDistanceFromOtherObjects = 1.5f;
    public LayerMask collisionMask;
    public bool skipCollisionCheck = false;

    [Header("Special Bumper")]
    public GameObject specialBumperPrefab;
    [Range(0f, 1f)]
    public float specialSpawnChance = 0.2f;

    BoxCollider spawnVolume;
    private bool poolInitialized = false;

    void Awake()
    {
        spawnVolume = GetComponent<BoxCollider>();
        spawnVolume.isTrigger = true;
    }

    void Start()
    {
        // Initialize the pool on first use
        InitializePool();
    }

    private void InitializePool()
    {
        if (poolInitialized) return;

        if (BumperPool.Instance != null)
        {
            BumperPool.Instance.Initialize();
            poolInitialized = true;
        }
    }

    /// <summary>
    /// Called by GameManager when a bumper is destroyed.
    /// </summary>
    public void RequestSpawn()
    {
        InitializePool();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        int spawned = 0;
        int safety = 0;
        int maxAttempts = skipCollisionCheck ? 10 : 50;

        while (spawned < bumpersPerBurst && safety < maxAttempts)
        {
            safety++;
            Vector3 candidatePosition = GetRandomPointInVolume();

            // Skip collision check if enabled (for non-overlapping spawn cubes)
            bool positionValid = skipCollisionCheck || IsPositionClear(candidatePosition);

            if (positionValid)
            {
                // Determine which prefab to use
                GameObject prefabToUse = bumperPrefab;
                if (specialBumperPrefab != null && Random.value < specialSpawnChance)
                    prefabToUse = specialBumperPrefab;

                // Get bumper from pool
                if (BumperPool.Instance != null)
                {
                    Bumper bumper = BumperPool.Instance.GetBumper(
                        candidatePosition,
                        prefabToUse.transform.rotation,
                        bumperParent ?? transform
                    );

                    if (bumper != null)
                    {
                        if (spawnerDebugLogging)
                            Debug.Log($"Spawned bumper from pool at {candidatePosition}");
                        spawned++;
                    }
                    else
                    {
                        if (spawnerDebugLogging)
                            Debug.LogWarning("Pool returned null bumper!");
                        break;
                    }
                }

                // Small delay between individual bumper spawns to prevent frame hiccups
                yield return null;
            }
            else
            {
                yield return null;
            }
        }

        if (spawnerDebugLogging && spawned < bumpersPerBurst)
            Debug.LogWarning($"SpawnRoutine only spawned {spawned}/{bumpersPerBurst} bumpers after {safety} attempts.");
    }

    Vector3 GetRandomPointInVolume()
    {
        Vector3 center = spawnVolume.bounds.center;
        Vector3 extents = spawnVolume.bounds.extents;

        return new Vector3(
            Random.Range(center.x - extents.x, center.x + extents.x),
            Random.Range(center.y - extents.y, center.y + extents.y),
            Random.Range(center.z - extents.z, center.z + extents.z)
        );
    }

    bool IsPositionClear(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(
            position,
            minDistanceFromOtherObjects,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        return hits.Length == 0;
    }

    void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
