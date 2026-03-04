using System.Collections;
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

    [Header("Scoring")]
    public int baseBumperPoints = 100;
    public float comboWindow = 1f;
    public int maxComboMultiplier = 5;


    [Header("Debug")]
    public bool verboseLogging = true;
    public int currentScore;
    public int CurrentScore => currentScore;
    public int currentComboMultiplier = 1;

    private float lastHitTime;

    [Header("Ball Spawn Control")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public Transform leftKickerEntry;
    public Transform rightKickerEntry;

    [Tooltip("True = Right side, False = Left side")]
    public bool[] spawnDirectives = new bool[60];

    public int spawnIndex = 0;

    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public float ballRespawnDelay = 2f;

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

    public (int points, int combo) RegisterBumperHit(int pointOverride = -1)
    {
        float timeSinceLastHit = Time.time - lastHitTime;
        if (timeSinceLastHit <= comboWindow)
            currentComboMultiplier = Mathf.Min(currentComboMultiplier + 1, maxComboMultiplier);
        else
            currentComboMultiplier = 1;

        int basePoints = pointOverride >= 0 ? pointOverride : baseBumperPoints;
        int pointsEarned = basePoints * currentComboMultiplier;
        currentScore += pointsEarned;
        lastHitTime = Time.time;

        return (pointsEarned, currentComboMultiplier);
    }

    /// <summary>
    /// Called by a bumper when it is destroyed.
    /// </summary>
    public void OnBumperDestroyed(Bumper bumper)
    {
        bumperSpawners.RemoveAll(s => s == null);

        if (bumperSpawners.Count == 0)
        {
            if (verboseLogging)
            {
                Debug.LogWarning("No valid spawners registered!");
                return;
            }
        }

        if (chooseRandomSpawner)
        {
            Spawner spawner = bumperSpawners[Random.Range(0, bumperSpawners.Count)];
            spawner.RequestSpawn();
        }
        else
        {
            foreach (var spawner in bumperSpawners)
                spawner.RequestSpawn();
        }
        if (verboseLogging)
        {
            Debug.Log($"OnBumperDestroyed called. List count: {bumperSpawners.Count}");
            for (int i = 0; i < bumperSpawners.Count; i++)
                Debug.Log($"Spawner[{i}]: {(bumperSpawners[i] == null ? "NULL" : bumperSpawners[i].name)}");
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
        StartCoroutine(RespawnBallAfterDelay());
    }

    private IEnumerator RespawnBallAfterDelay()
    {
        yield return new WaitForSeconds(ballRespawnDelay);
        bool spawnRight = GetNextSpawnSide();
        SpawnBall(spawnRight);
    }

}
