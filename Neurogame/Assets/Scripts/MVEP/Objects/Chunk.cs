using System;
using UnityEngine;

public class Chunk : MonoBehaviour, IPoolable<Chunk>
{
  [SerializeField] private Transform[] laneAnchors;
  public Transform[] LaneAnchors => laneAnchors;
  [SerializeField] private RiverBank[] riverBanks;
  public RiverBank[] RiverBanks => riverBanks;

  private bool active;
  private bool passed;

  private int obstacleLane;
  private int powerUpLane;

  private float chunkLength;
  private float moveSpeed;
  private ObjectPool<Chunk> pool;

  public int ObstacleLane => obstacleLane;
  public int PowerUpLane => powerUpLane;

  public static Action<Chunk> OnChunkActivated;
  public static Action<Chunk> OnChunkPassed;

  void OnEnable()
  {
    MVEPGameEvents.OnSpeedChanged += SetSpeed;
  }

  void OnDisable()
  {
    MVEPGameEvents.OnSpeedChanged -= SetSpeed;
  }

  public void SetPool(ObjectPool<Chunk> pool)
  {
    this.pool = pool;
  }

  public void Initialize(float speed, Vector3 startPosition, float length)
  {
    moveSpeed = speed;
    chunkLength = length;
    transform.position = startPosition;
    gameObject.SetActive(true);
  }

  public void SetSpeed(float speed)
  {
    moveSpeed = speed;
  }

  public void SetLanes(int obstacleLane, int powerUpLane)
  {
    this.obstacleLane = obstacleLane;
    this.powerUpLane = powerUpLane;
  }

  public void Tick(float deltaTime)
  {
    float moveAmount = moveSpeed * deltaTime;
    transform.Translate(Vector3.back * moveAmount, Space.World);

    CheckActive();
    if (active) CheckPassed();
  }

  public void Reset()
  {
    active = false;
    passed = false;
  }

  public void ReturnToPool()
  {
    gameObject.SetActive(false);
    pool.Return(this);
  }

  public (int, int) GetLanes()
  {
    return (obstacleLane, powerUpLane);
  }

  private void CheckActive()
  {
    if (active) return;
    if (transform.position.z <= 0f)
    {
      OnChunkActivated?.Invoke(this);
      active = true;
    }
  }

  private void CheckPassed()
  {
    if (passed) return;
    if (transform.position.z <= -chunkLength)
    {
      OnChunkPassed?.Invoke(this);
      passed = true;
    }
  }
}
