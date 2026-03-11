using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the lifecycle of chunks in the game world.
/// Handles chunk spawning, recycling, speed management, and object pooling.
/// Uses an infinite generation pattern to create chunks in sequence.
/// </summary>
[RequireComponent(typeof(ChunkDesigner))]
public class ChunkManager : MonoBehaviour
{
  // Constants
  private const int DEFAULT_CHUNK_POOL_SIZE = 6;
  private const int DEFAULT_OBSTACLE_POOL_SIZE = 10;
  private const int DEFAULT_POWERUP_POOL_SIZE = 10;
  private const int DEFAULT_CHUNKS_AHEAD = 6;
  private const float DEFAULT_RECYCLE_THRESHOLD = 34f;
  private const int INVALID_LANE = -1;

  // Configuration
  private ChunkConfiguration chunkConfig;
  private float chunkLength;
  private int startingChunks;
  private int chunksAhead;
  private float recycleThreshold;

  // Component References
  private ChunkDesigner chunkDesigner;

  // Object Pools
  private ObjectPool<Chunk> chunkPool;
  private ObjectPool<Obstacle> obstaclePool;
  private ObjectPool<PowerUp> powerUpPool;

  // Runtime State
  private readonly List<Chunk> activeChunks = new();
  private float chunkSpeed;
  private bool gameOn = false;

  private void Awake()
  {
    chunksAhead = DEFAULT_CHUNKS_AHEAD;
    recycleThreshold = DEFAULT_RECYCLE_THRESHOLD;

    chunkDesigner = GetComponent<ChunkDesigner>();
  }

  private void OnEnable()
  {
    MVEPGameEvents.OnGameStarted += StartGame;
    MVEPGameEvents.OnGamePaused += () => { gameOn = false; };
    MVEPGameEvents.OnGameResumed += () => { gameOn = true; };
    MVEPGameEvents.OnGameEnded += EndGame;
  }

  private void Start()
  {
    startingChunks = MVEPGameManager.Instance.gameConfig.WarmUpChunks;
    chunkConfig = MVEPGameManager.Instance.chunkConfig;
    chunkLength = MVEPGameManager.Instance.timingConfig.ChunkLength;

    chunkSpeed = chunkLength / MVEPGameManager.Instance.timingConfig.TotalTime;
    MVEPGameEvents.OnSpeedChanged?.Invoke(chunkSpeed);

    chunkPool = new ObjectPool<Chunk>(chunkConfig.chunkPrefab, DEFAULT_CHUNK_POOL_SIZE, transform);
    obstaclePool = new ObjectPool<Obstacle>(chunkConfig.obstaclePrefab, DEFAULT_OBSTACLE_POOL_SIZE, transform);
    powerUpPool = new ObjectPool<PowerUp>(chunkConfig.powerUpPrefab, DEFAULT_POWERUP_POOL_SIZE, transform);
  }

  /// <summary>
  /// Initializes the chunk queue with starting blank chunks and designed chunks.
  /// </summary>
  public void SpawnInitialChunks()
  {
    // Spawn initial blank chunks (no obstacles/power-ups)
    for (int i = 0; i < startingChunks; i++)
    {
      SpawnChunk(i, designChunk: false);
    }

    // Spawn designed chunks ahead
    for (int i = startingChunks; i < chunksAhead; i++)
    {
      SpawnChunk(i, designChunk: true);
    }
  }

  public void StartGame()
  {
    SpawnInitialChunks();
    gameOn = true;
  }

  /// <summary>
  /// Updates all active chunks and recycles old ones.
  /// </summary>
  private void Update()
  {
    if (!gameOn) return;
    UpdateActiveChunks(Time.deltaTime);
    RecycleChunksIfNeeded();
  }

  /// <summary>
  /// Spawns a chunk at the given index position, optionally with obstacles and power-ups.
  /// </summary>
  /// <param name="index">The chunk index in the sequence.</param>
  /// <param name="designChunk">If true, populates the chunk with obstacles/power-ups.</param>
  private void SpawnChunk(int index, bool designChunk)
  {
    Chunk chunk = chunkPool.Get();
    Vector3 position = GetChunkPosition(index);

    chunk.Initialize(chunkSpeed, position, chunkLength);

    if (designChunk)
    {
      chunkDesigner.DesignChunk(chunk, obstaclePool, powerUpPool);
    }
    else
    {
      chunk.SetLanes(INVALID_LANE, INVALID_LANE);
    }

    activeChunks.Add(chunk);
  }

  /// <summary>
  /// Calculates the world position of a chunk by its index.
  /// </summary>
  /// <param name="index">The chunk index.</param>
  /// <returns>World position for the chunk.</returns>
  private Vector3 GetChunkPosition(int index)
  {
    return transform.localPosition + chunkLength * index * Vector3.forward;
  }

  /// <summary>
  /// Updates all active chunks by one tick.
  /// </summary>
  /// <param name="deltaTime">Delta time since last frame.</param>
  private void UpdateActiveChunks(float deltaTime)
  {
    for (int i = 0; i < activeChunks.Count; i++)
    {
      activeChunks[i].Tick(deltaTime);
    }
  }

  /// <summary>
  /// Checks and recycles the first chunk if it has passed the recycle threshold.
  /// </summary>
  private void RecycleChunksIfNeeded()
  {
    if (activeChunks.Count == 0)
      return;

    Chunk firstChunk = activeChunks[0];

    if (firstChunk.transform.localPosition.z <= -recycleThreshold)
    {
      RecycleChunk(firstChunk);
    }
  }

  /// <summary>
  /// Recycles a chunk by removing it from active list, redesigning it,
  /// and placing it at the end of the chunk sequence.
  /// </summary>
  /// <param name="chunk">The chunk to recycle.</param>
  private void RecycleChunk(Chunk chunk)
  {
    activeChunks.Remove(chunk);
    chunkDesigner.DesignChunk(chunk, obstaclePool, powerUpPool);

    float newZ = activeChunks[^1].transform.localPosition.z + chunkLength;
    chunk.transform.localPosition = new Vector3(0, 0, newZ);
    chunk.SetSpeed(chunkSpeed);
    chunk.Reset();

    activeChunks.Add(chunk);
  }

  /// <summary>
  /// Updates the speed of all chunks. Called when game speed changes.
  /// </summary>
  /// <param name="speed">The new chunk speed.</param>
  public void SetSpeed(float speed)
  {
    if (speed <= 0)
    {
      Debug.LogWarning("ChunkManager: Speed must be positive");
      return;
    }

    chunkSpeed = speed;

    foreach (var chunk in activeChunks)
    {
      chunk.SetSpeed(speed);
    }
  }

  public void ClearChunks()
  {
    foreach (var chunk in activeChunks)
    {
      chunkPool.Return(chunk);
    }
    activeChunks.Clear();
  }

  public void EndGame()
  {
    ClearChunks();
    gameOn = false;
  }
}
