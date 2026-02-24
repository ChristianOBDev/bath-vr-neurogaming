using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ChunkDesigner))]
public class ChunkManager : MonoBehaviour
{
  [Header("Component References")]
  private ChunkDesigner chunkDesigner;

  [Header("Prefabs")]
  [SerializeField] private Obstacle obstaclePrefab;
  [SerializeField] private PowerUp powerUpPrefab;

  [Header("Settings")]
  private ChunkConfiguration chunkConfig;
  [SerializeField] private int startingChunks = 2;
  [SerializeField] private int chunksAhead = 6;
  private float chunkLength;
  private float chunkSpeed;
  private readonly int initialChunkPoolSize = 10;
  private readonly int initialObstaclePoolSize = 20;
  private readonly int initialPowerUpPoolSize = 20;

  [Header("Pools")]
  private ObjectPool<Chunk> chunkPool;
  private ObjectPool<Obstacle> obstaclePool;
  private ObjectPool<PowerUp> powerUpPool;

  [Header("Runtime")]
  private readonly List<Chunk> activeChunks = new();

  private void Awake()
  {
    chunkConfig = MVEPGameSettings.Instance.chunkConfig;
    chunkLength = chunkConfig.chunkLength;
    chunkSpeed = chunkLength / chunkConfig.chunkTraversalTime;
    MVEPGameEvents.OnSpeedChanged?.Invoke(chunkSpeed);

    chunkDesigner = GetComponent<ChunkDesigner>();

    chunkPool = new ObjectPool<Chunk>(chunkConfig.chunkPrefab, initialChunkPoolSize, transform);
    obstaclePool = new ObjectPool<Obstacle>(obstaclePrefab, initialObstaclePoolSize, transform);
    powerUpPool = new ObjectPool<PowerUp>(powerUpPrefab, initialPowerUpPoolSize, transform);
  }

  private void Start()
  {
    for (int i = 0; i < startingChunks; i++)
    {
      SpawnBlankChunk(i);
    }

    for (int i = startingChunks; i < chunksAhead; i++)
    {
      SpawnInitialChunk(i);
    }
  }

  private void Update()
  {
    TickChunks(Time.deltaTime);
  }

  private void SpawnBlankChunk(int index)
  {
    Chunk chunk = chunkPool.Get();
    Vector3 pos = chunkLength * index * Vector3.forward;

    chunk.Initialize(chunkSpeed, pos, chunkLength);
    chunk.SetLanes(-1, -1);
    activeChunks.Add(chunk);
  }

  private void SpawnInitialChunk(int index)
  {
    Chunk chunk = chunkPool.Get();
    Vector3 pos = chunkLength * index * Vector3.forward;

    chunk.Initialize(chunkSpeed, pos, chunkLength);

    chunkDesigner.DesignChunk(chunk, obstaclePool, powerUpPool);

    activeChunks.Add(chunk);
  }

  private void TickChunks(float deltaTime)
  {
    for (int i = 0; i < activeChunks.Count; i++)
    {
      activeChunks[i].Tick(deltaTime);
    }

    Chunk first = activeChunks[0];

    if (first.transform.position.z <= -chunkLength * 1.5f)
    {
      RecycleChunk(first);
    }
  }

  private void RecycleChunk(Chunk chunk)
  {
    activeChunks.Remove(chunk);

    chunkDesigner.DesignChunk(chunk, obstaclePool, powerUpPool);

    float newZ = activeChunks[^1].transform.position.z + chunkLength;

    chunk.transform.position = new Vector3(0, 0, newZ);
    chunk.SetSpeed(chunkSpeed);

    chunk.Reset();

    activeChunks.Add(chunk);
  }

  public void SetSpeed(float speed)
  {
    chunkSpeed = speed;

    foreach (var chunk in activeChunks)
    {
      chunk.SetSpeed(speed);
    }
  }
}
