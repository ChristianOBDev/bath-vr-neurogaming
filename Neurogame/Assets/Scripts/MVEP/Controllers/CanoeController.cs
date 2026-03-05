using UnityEngine;

public class CanoeController : MonoBehaviour
{
  private LaneConfiguration laneConfig;
  private ChunkConfiguration chunkConfig;
  private float laneChangeDuration;

  private int targetLane = 2;
  private int currentLane = 2; // Start in the middle lane (index 2)

  private Phase phase;

  void Awake()
  {
    laneConfig = MVEPGameSettings.Instance.laneConfig;
    chunkConfig = MVEPGameSettings.Instance.chunkConfig;
    phase = MVEPGameSettings.Instance.CurrentPhase;
  }

  void OnEnable()
  {
    Chunk.OnChunkActivated += GetActiveChunk;
  }

  void OnDisable()
  {
    Chunk.OnChunkActivated -= GetActiveChunk;
  }

  void Start()
  {
    float remainingTime = chunkConfig.chunkTraversalTime - MVEPGameSettings.Instance.mvepConfig.StimuliDuration;
    laneChangeDuration = 0.66f * remainingTime;
  }

  public void SetTargetLane(int laneIndex)
  {
    if (currentLane == laneIndex) return; // No need to change lanes if already in the target lane
    targetLane = Mathf.Clamp(laneIndex, 0, laneConfig.laneCount - 1);
  }

  public void GetActiveChunk(Chunk chunk)
  {
    if (chunk.ObstacleLane < 0 || chunk.ObstacleLane >= laneConfig.laneCount || chunk.PowerUpLane < 0 || chunk.PowerUpLane >= laneConfig.laneCount)
    {
      return;
    }

    switch (phase)
    {
      case Phase.NoControl:
        SetTargetLane(chunk.PowerUpLane); // Immediately set target lane to power-up lane, but only change lanes on pulse complete
        break;
      case Phase.HalfControl:
        SetTargetLane(chunk.PowerUpLane); // Immediately set target lane to power-up lane, but only change lanes on pulse complete
        break;
      case Phase.FullControl:
        int randomLane;
        do
        {
          randomLane = Random.Range(0, laneConfig.laneCount);
        }
        while (randomLane == chunk.ObstacleLane || randomLane == chunk.PowerUpLane); // Ensure the random lane is different from the current lane, obstacle lane, and power-up lane
        SetTargetLane(randomLane);
        break;
    }
  }

  public void ChangeLanes()
  {
    float targetX = laneConfig.GetLaneXPosition(targetLane);
    int distance = Mathf.Abs(targetLane - currentLane);
    transform.LeanMoveX(targetX, laneChangeDuration).setEaseInOutSine().setOnComplete(() => currentLane = targetLane);
  }

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Obstacle"))
    {
      RockTheBoat();
      MVEPGameEvents.OnObstacleHit?.Invoke();
    }
    else if (other.CompareTag("PowerUp"))
    {
      SpeedUp();
      MVEPGameEvents.OnPowerUpCollected?.Invoke();
    }
  }

  private void RockTheBoat()
  {
    float randomX = Random.Range(-0.5f, 0.5f); // Random horizontal offset for rocking
    transform.LeanMoveX(transform.position.x + randomX, 0.2f).setEaseInOutSine().setLoopPingPong(2);
    transform.LeanRotateZ(15f, 0.2f).setLoopPingPong(2).setEaseInOutSine();
  }

  private void SpeedUp()
  {

  }
}
