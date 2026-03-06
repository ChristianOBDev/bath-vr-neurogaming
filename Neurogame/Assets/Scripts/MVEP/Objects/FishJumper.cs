using UnityEngine;

/// <summary>
/// Manages a fish character that jumps and moves to different lanes.
/// Responds to chunk activation events to move to power-up lanes,
/// and jumps when the canoe returns to center.
/// </summary>
public class FishJumper : MonoBehaviour
{
  // Constants
  private const float LANE_MOVE_DURATION = 0.1f;
  private const float JUMP_ROTATION_ANGLE = 60f;
  private const float JUMP_UP_DURATION = 1f;
  private const float JUMP_DOWN_DURATION = 0f;

  // Configuration
  private LaneConfiguration laneConfig;

  // References
  [SerializeField] private GameObject fish;

  /// <summary>
  /// Initializes lane configuration from game settings.
  /// </summary>
  private void Start()
  {
    laneConfig = MVEPGameSettings.Instance.laneConfig;

    if (fish == null)
    {
      Debug.LogError("FishJumper: Fish game object not assigned!");
    }
  }

  /// <summary>
  /// Subscribes to game events.
  /// </summary>
  private void OnEnable()
  {
    MVEPGameEvents.OnChunkActivated += HandleChunkActivated;
    MVEPGameEvents.OnCanoeCentered += HandleCanoeCentered;
  }

  /// <summary>
  /// Unsubscribes from game events.
  /// </summary>
  private void OnDisable()
  {
    MVEPGameEvents.OnChunkActivated -= HandleChunkActivated;
    MVEPGameEvents.OnCanoeCentered -= HandleCanoeCentered;
  }

  /// <summary>
  /// Handles chunk activation by moving the fish to the power-up lane.
  /// </summary>
  /// <param name="chunk">The newly activated chunk.</param>
  private void HandleChunkActivated(Chunk chunk)
  {
    if (!IsValidLane(chunk.PowerUpLane))
      return;

    MoveFishToLane(chunk.PowerUpLane);
  }

  /// <summary>
  /// Handles canoe centering by triggering a jump animation.
  /// </summary>
  private void HandleCanoeCentered()
  {
    PerformJump();
  }

  /// <summary>
  /// Moves the fish to the specified lane position.
  /// </summary>
  /// <param name="laneIndex">The target lane index.</param>
  private void MoveFishToLane(int laneIndex)
  {
    float targetX = laneConfig.GetLaneXPosition(laneIndex);
    transform.LeanMoveLocalX(targetX, LANE_MOVE_DURATION);
  }

  /// <summary>
  /// Performs the jump animation sequence.
  /// Shows the fish, rotates up, then resets and hides.
  /// </summary>
  private void PerformJump()
  {
    if (fish == null)
      return;

    fish.SetActive(true);

    transform.LeanRotateX(JUMP_ROTATION_ANGLE, JUMP_UP_DURATION)
      .setEaseInOutSine()
      .setOnComplete(() => ResetJump());
  }

  /// <summary>
  /// Resets the jump animation by rotating back to neutral and hiding the fish.
  /// </summary>
  private void ResetJump()
  {
    transform.LeanRotateX(-JUMP_ROTATION_ANGLE, JUMP_DOWN_DURATION);

    if (fish != null)
    {
      fish.SetActive(false);
    }
  }

  /// <summary>
  /// Validates if a lane index is within the valid range.
  /// </summary>
  /// <param name="laneIndex">The lane index to validate.</param>
  /// <returns>True if the lane is valid, false otherwise.</returns>
  private bool IsValidLane(int laneIndex)
  {
    return laneIndex >= 0 && laneIndex < laneConfig.laneCount;
  }
}
