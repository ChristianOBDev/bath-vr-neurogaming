using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
  public XRRigProfileTrigger rigProfileTrigger;

  [Header("Bumper Spawner Control")]
  public List<Spawner> bumperSpawners = new List<Spawner>();

  [Header("Bumper Spawn Selection")]
  [Tooltip("If true, choose one random spawner per bumper destruction")]
  public bool chooseRandomSpawner = true;

  [Header("Bumper Population Control")]
  public int minimumBumperCount = 8;
  public float bumperCheckInterval = 2f;
  private float bumperCheckTimer = 0f;

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

  [Header("Kicker References")]
  public KickerForce leftKickerForce;
  public KickerForce rightKickerForce;

  [Tooltip("True = Right side, False = Left side")]
  public bool[] spawnDirectives = new bool[60];

  public int spawnIndex = 0;

  [Header("Ball Settings")]
  public GameObject ballPrefab;
  public float ballRespawnDelay = 2f;
  public Transform waterfallCenter;

  BallController currentBall;

  private Coroutine respawnCoroutine;

  private bool gameRunning = false;
  public bool GameRunning => gameRunning;

  void Start()
  {
    // bool spawnRight = GetNextSpawnSide();
    // SpawnBall(spawnRight);
  }

  void Update()
  {
    bumperCheckTimer += Time.deltaTime;
    if (bumperCheckTimer >= bumperCheckInterval)
    {
      bumperCheckTimer = 0f;
      CheckBumperPopulation();
    }
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
        Debug.Log($"OnBumperDestroyed called. List count: {bumperSpawners.Count}");
        for (int i = 0; i < bumperSpawners.Count; i++)
          Debug.Log($"Spawner[{i}]: {(bumperSpawners[i] == null ? "NULL" : bumperSpawners[i].name)}");
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
  void CheckBumperPopulation()
  {
    int activeBumpers = FindObjectsByType<Bumper>(FindObjectsSortMode.None).Length;

    if (verboseLogging)
      Debug.Log($"Active bumpers: {activeBumpers} / minimum: {minimumBumperCount}");

    if (activeBumpers < minimumBumperCount)
    {
      int deficit = minimumBumperCount - activeBumpers;

      if (verboseLogging)
        Debug.Log($"Bumper deficit of {deficit}, requesting spawns.");

      for (int i = 0; i < deficit; i++)
      {
        if (bumperSpawners.Count == 0) break;
        Spawner spawner = bumperSpawners[Random.Range(0, bumperSpawners.Count)];
        if (spawner != null)
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

    // Pass scene reference to spawned ball
    if (waterfallCenter != null)
      currentBall.waterfallCenter = waterfallCenter;

    Vector3 targetPos = spawnRight
        ? rightKickerEntry.position
        : leftKickerEntry.position;

    currentBall.BeginReturnPhase(targetPos);

    // Notify the correct kicker to begin glow build
    KickerForce targetKicker = spawnRight ? rightKickerForce : leftKickerForce;
    if (targetKicker != null)
      targetKicker.BeginGlowBuild(currentBall.returnDuration);
  }

  public void HandleBallDeath(BallController ball)
  {
    Destroy(ball.gameObject);
    currentBall = null;
    if (respawnCoroutine != null)
      StopCoroutine(respawnCoroutine);
    respawnCoroutine = StartCoroutine(RespawnBallAfterDelay());
  }

  private IEnumerator RespawnBallAfterDelay()
  {
    yield return new WaitForSeconds(ballRespawnDelay);
    bool spawnRight = GetNextSpawnSide();
    SpawnBall(spawnRight);
  }

  public void ResetAndRespawn()
  {
    // Cancel any pending delayed respawn
    if (respawnCoroutine != null)
    {
      StopCoroutine(respawnCoroutine);
      respawnCoroutine = null;
    }

    spawnIndex = 0;

    if (currentBall != null)
    {
      Destroy(currentBall.gameObject);
      currentBall = null;
    }

    bool spawnRight = GetNextSpawnSide();
    SpawnBall(spawnRight);

    Debug.Log("Spawn index reset and ball respawned.");
  }

  public void StartGame()
  {
    ResetAndRespawn();
    gameRunning = true;
  }

  public void PauseGame()
  {
    gameRunning = false;
  }

  public void ResumeGame()
  {
    gameRunning = true;
  }

  public void ResetGame()
  {
    currentScore = 0;
    currentComboMultiplier = 1;
    lastHitTime = 0f;
    // ResetAndRespawn();
    gameRunning = true;
  }

  public void QuitGame()
  {
    Debug.Log("QuitGame called. Implement platform-specific quit logic here.");
    rigProfileTrigger.Reset();
  }
}
