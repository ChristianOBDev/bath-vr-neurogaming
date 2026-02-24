using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Bumper Spawner Control")]
    public List<Spawner> bumperSpawners = new List<Spawner>();

    [Header("Bumper Spawn Selection")]
    [Tooltip("If true, choose one random spawner per bumper destruction")]
    public bool chooseRandomSpawner = true;

    [Header("Ball Spawn Control")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public Transform leftKickerEntry;
    public Transform rightKickerEntry;

    [Tooltip("True = Right side, False = Left side")]
    public bool[] spawnDirectives = new bool[60];

    int spawnIndex = 0;

    [Header("Ball Settings")]
    public GameObject ballPrefab;

    BallController currentBall;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    void Start()
    {
        bool spawnRight = GetNextSpawnSide();
        SpawnBall(spawnRight);
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

    public bool GetNextSpawnSide()
    {
        if (spawnDirectives == null || spawnDirectives.Length == 0)
            return false;

        bool side = spawnDirectives[spawnIndex];

        spawnIndex = (spawnIndex + 1) % spawnDirectives.Length;

        return side;
    }

    public void SpawnBall(bool spawnRight)
    {
        Transform spawnPoint = spawnRight
            ? rightSpawnPoint
            : leftSpawnPoint;

        GameObject ballObj = Instantiate(
            ballPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        currentBall = ballObj.GetComponent<BallController>();

        Vector3 targetPos = spawnRight
            ? rightKickerEntry.position
            : leftKickerEntry.position;

        currentBall.BeginReturnPhase(targetPos);
    }


    public void HandleBallDeath(BallController ball)
    {
        Destroy(ball.gameObject);

        bool spawnRight = GetNextSpawnSide();

        SpawnBall(spawnRight);
    }

}
