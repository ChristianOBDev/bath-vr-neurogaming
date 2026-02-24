using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour
{
    [Header("Spawned Object")]
    public GameObject bumperPrefab;

    [Header("Spawn Settings")]
    [Min(0f)]
    public float respawnDelay = 1f;

    [Min(1)]
    public int bumpersPerBurst = 1;

    [Header("Placement Rules")]
    public float minDistanceFromOtherObjects = 1.5f;
    public LayerMask collisionMask;

    BoxCollider spawnVolume;

    void Awake()
    {
        spawnVolume = GetComponent<BoxCollider>();
        spawnVolume.isTrigger = true;
    }

    /// <summary>
    /// Called by GameManager when a bumper is destroyed.
    /// </summary>
    public void RequestSpawn()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        int spawned = 0;
        int safety = 0;

        while (spawned < bumpersPerBurst && safety < 50)
        {
            safety++;

            Vector3 candidatePosition = GetRandomPointInVolume();

            if (IsPositionClear(candidatePosition))
            {
                Instantiate(bumperPrefab, candidatePosition, Quaternion.identity);
                spawned++;
            }

            yield return null; // spread work over frames
        }
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
