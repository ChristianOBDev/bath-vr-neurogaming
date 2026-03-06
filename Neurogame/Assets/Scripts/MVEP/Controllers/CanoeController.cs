using UnityEngine;

public class CanoeController : MonoBehaviour
{
  // Constants
  private const int CENTER_LANE = 2;
  private const float ROCK_OFFSET_RANGE = 0.5f;
  private const float ROCK_DURATION = 0.2f;
  private const float ROCK_ANGLE = 15f;

  // References
  private LaneConfiguration laneConfig;
  private Phase phase;

  // State
  private float laneChangeDuration;
  private int targetLane = CENTER_LANE;
  private int currentLane = CENTER_LANE;

  void Awake()
  {
    laneConfig = MVEPGameSettings.Instance.laneConfig;
    phase = MVEPGameSettings.Instance.CurrentPhase;
  }

  void OnEnable()
  {
    MVEPGameEvents.OnChunkActivated += GetActiveChunk;
    MVEPGameEvents.OnChunkPassed += OnChunkPassed;
  }

  void OnDisable()
  {
    MVEPGameEvents.OnChunkActivated -= GetActiveChunk;
    MVEPGameEvents.OnChunkPassed -= OnChunkPassed;
  }

  void Start()
  {
    laneChangeDuration = MVEPGameSettings.Instance.timingConfig.LaneChangeDuration;
  }

  /// <summary>
  /// Sets the target lane for the canoe to move towards.
  /// </summary>
  /// <param name="laneIndex">The desired lane index.</param>
  public void SetTargetLane(int laneIndex)
  {
    if (currentLane == laneIndex) return;
    targetLane = Mathf.Clamp(laneIndex, 0, laneConfig.laneCount - 1);
  }

  /// <summary>
  /// Handles chunk activation and determines the appropriate target lane based on game phase.
  /// </summary>
  /// <param name="chunk">The activated chunk.</param>
  public void GetActiveChunk(Chunk chunk)
  {
    if (chunk.ObstacleLane < 0 || chunk.ObstacleLane >= laneConfig.laneCount || chunk.PowerUpLane < 0 || chunk.PowerUpLane >= laneConfig.laneCount)
    {
      return;
    }

    switch (phase)
    {
      case Phase.NoControl:
      case Phase.HalfControl:
        SetTargetLane(chunk.PowerUpLane);
        break;
      case Phase.FullControl:
        int randomLane;
        do
        {
          randomLane = Random.Range(0, laneConfig.laneCount);
        }
        while (randomLane == chunk.ObstacleLane || randomLane == chunk.PowerUpLane);
        SetTargetLane(randomLane);
        break;
    }
  }

  /// <summary>
  /// Moves the canoe to the target lane using a smooth eased animation.
  /// </summary>
  public void ChangeLanes()
  {
    ChangeLanes(targetLane);
  }

  /// <summary>
  /// Moves the canoe to a specific lane using a smooth eased animation.
  /// </summary>
  /// <param name="laneIndex">The lane to move to.</param>
  public void ChangeLanes(int laneIndex)
  {
    laneIndex = Mathf.Clamp(laneIndex, 0, laneConfig.laneCount - 1);
    PerformLaneChange(laneIndex, laneChangeDuration);
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
      MVEPGameEvents.OnPowerUpCollected?.Invoke();
    }
  }

  /// <summary>
  /// Returns the canoe to the center lane with a smooth animation.
  /// </summary>
  public void BackToCenter()
  {
    PerformLaneChange(CENTER_LANE, MVEPGameSettings.Instance.timingConfig.LaneResetDuration, () =>
    {
      currentLane = targetLane;
      MVEPGameEvents.OnCanoeCentered?.Invoke();
    });
  }

  /// <summary>
  /// Callback for when a chunk has passed. Triggers return to center.
  /// </summary>
  /// <param name="context">The chunk context.</param>
  private void OnChunkPassed(object context)
  {
    BackToCenter();
  }

  /// <summary>
  /// Performs the actual lane change animation.
  /// </summary>
  /// <param name="targetLaneIndex">The target lane index.</param>
  /// <param name="duration">Animation duration in seconds.</param>
  /// <param name="onComplete">Optional callback when animation completes.</param>
  private void PerformLaneChange(int targetLaneIndex, float duration, System.Action onComplete = null)
  {
    float targetX = laneConfig.GetLaneXPosition(targetLaneIndex);
    LTDescr tween = transform.LeanMoveX(targetX, duration)
      .setEaseInOutSine()
      .setOnComplete(() =>
      {
        currentLane = targetLaneIndex;
        onComplete?.Invoke();
      });
  }

  /// <summary>
  /// Rocks the canoe back and forth as a reaction to hitting an obstacle.
  /// </summary>
  private void RockTheBoat()
  {
    float randomX = Random.Range(-ROCK_OFFSET_RANGE, ROCK_OFFSET_RANGE);
    transform.LeanMoveX(transform.position.x + randomX, ROCK_DURATION)
      .setEaseInOutSine()
      .setLoopPingPong(2);
    transform.LeanRotateZ(ROCK_ANGLE, ROCK_DURATION)
      .setLoopPingPong(2)
      .setEaseInOutSine();
  }
}
