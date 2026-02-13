using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawner Control")]
    public List<Spawner> bumperSpawners = new List<Spawner>();

    [Header("Spawn Selection")]
    [Tooltip("If true, choose one random spawner per bumper destruction")]
    public bool chooseRandomSpawner = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Called by a bumper when it is destroyed.
    /// </summary>
    public void OnBumperDestroyed(Bumper bumper)
    {

        Debug.Log($"Bumper destroyed: {bumper.name}");

        if (bumperSpawners.Count == 0)
        {
            Debug.LogWarning("No spawners registered!");
            return;
        }

        if (bumperSpawners.Count == 0)
            return;

        if (chooseRandomSpawner)
        {
            Spawner spawner =
                bumperSpawners[Random.Range(0, bumperSpawners.Count)];

            spawner.RequestSpawn();
        }
        else
        {
            // Trigger all spawners (optional behavior)
            foreach (var spawner in bumperSpawners)
            {
                spawner.RequestSpawn();
            }
        }
    }
}
