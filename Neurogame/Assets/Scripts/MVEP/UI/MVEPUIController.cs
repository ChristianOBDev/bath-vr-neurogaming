using UnityEngine;

/// <summary>
/// Controls the MVEP UI elements including stimulus arrows and pulse timing.
/// Shows directional arrows when chunks activate and manages stimulus presentation.
/// Coordinates between game events and the stimulus controller.
/// </summary>
public class MVEPUIController : MonoBehaviour
{
  // Constants
  private const int INVALID_LANE = -1;

  // Configuration - References
  [SerializeField] private MVEPStimuliController stimuliController;
  [SerializeField] private GameObject[] arrows;

  /// <summary>
  /// Subscribes to game events.
  /// </summary>
  private void OnEnable()
  {
    MVEPGameEvents.OnChunkActivated += HandleChunkActivated;
    MVEPGameEvents.OnCanoeCentered += HandleCanoeCentered;
    MVEPStimuliController.PulseComplete += HandlePulseComplete;
  }

  /// <summary>
  /// Unsubscribes from game events.
  /// </summary>
  private void OnDisable()
  {
    MVEPGameEvents.OnChunkActivated -= HandleChunkActivated;
    MVEPGameEvents.OnCanoeCentered -= HandleCanoeCentered;
    MVEPStimuliController.PulseComplete -= HandlePulseComplete;
  }

  /// <summary>
  /// Handles chunk activation by displaying the arrow for the power-up lane.
  /// Hides all arrows and only shows the one pointing to the target lane.
  /// </summary>
  /// <param name="chunk">The newly activated chunk.</param>
  private void HandleChunkActivated(Chunk chunk)
  {
    if (!chunk.IsValid())
    {
      HideAllArrows();
      return;
    }

    ShowArrowForLane(chunk.PowerUpLane);
  }

  /// <summary>
  /// Handles canoe centering by initiating stimulus presentation.
  /// Includes the configured arrow preactivation delay.
  /// </summary>
  private void HandleCanoeCentered()
  {
    if (stimuliController == null)
    {
      Debug.LogError("MVEPUIController: Stimuli controller not assigned!");
      return;
    }

    float arrowPreactivationDuration = MVEPGameManager.Instance.timingConfig.ArrowPreactivationDuration;
    stimuliController.Pulse(arrowPreactivationDuration);
  }

  /// <summary>
  /// Handles the completion of a stimulus pulse sequence.
  /// Hides all directional arrows.
  /// </summary>
  private void HandlePulseComplete()
  {
    HideAllArrows();
  }

  /// <summary>
  /// Shows the arrow for a specific lane and hides all others.
  /// </summary>
  /// <param name="laneIndex">The lane index to show the arrow for.</param>
  private void ShowArrowForLane(int laneIndex)
  {
    HideAllArrows();

    if (laneIndex >= 0 && laneIndex < arrows.Length)
    {
      arrows[laneIndex].SetActive(true);
    }
  }

  /// <summary>
  /// Hides all directional arrows.
  /// </summary>
  private void HideAllArrows()
  {
    foreach (var arrow in arrows)
    {
      if (arrow != null)
      {
        arrow.SetActive(false);
      }
    }
  }
}
