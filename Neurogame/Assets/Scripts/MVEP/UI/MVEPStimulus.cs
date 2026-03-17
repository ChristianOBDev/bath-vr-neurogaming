using System;
using UnityEngine;

/// <summary>
/// Represents a single visual stimulus in the MVEP stimulus array.
/// Animates a line across the screen and broadcasts when the animation completes.
/// Each stimulus has a unique index for tracking which stimulus was presented.
/// </summary>
public class MVEPStimulus : MonoBehaviour
{
  // Constants - Animation Positions
  private const float START_POSITION_X = 45f;
  private const float END_POSITION_X = -45f;
  private const float POSITION_Y = 0f;

  // Configuration - Timing
  private float pulseDuration;

  // Configuration - References
  [SerializeField] private RectTransform line;

  // Configuration - Animation
  [SerializeField] private Vector2 startPos = new(START_POSITION_X, POSITION_Y);
  [SerializeField] private Vector2 endPos = new(END_POSITION_X, POSITION_Y);

  // Configuration - Identity
  [SerializeField] private int stimulusIndex;

  // Events
  /// <summary>
  /// Fired when a stimulus pulse animation completes.
  /// Broadcasts the stimulus index so listeners know which stimulus was presented.
  /// </summary>
  public static Action<int> OnStimulusPulsed;

  /// <summary>
  /// Initializes stimulus configuration from game settings.
  /// </summary>
  private void Start()
  {
    pulseDuration = MVEPGameManager.Instance.timingConfig.PulseDuration;
  }

  /// <summary>
  /// Executes the stimulus pulse animation.
  /// Animates the line from start to end position and fires event when complete.
  /// </summary>
  public void Pulse()
  {
    // Reset to start position and show
    line.anchoredPosition = startPos;
    line.gameObject.SetActive(true);

    // Animate across the screen
    line.LeanMoveLocalX(endPos.x, pulseDuration)
      .setOnComplete(OnPulseComplete);

    //Send UDP signal
    UDPManager.Instance.Send(stimulusIndex);
  }

  /// <summary>
  /// Handles the completion of the pulse animation.
  /// Hides the line, resets position, and broadcasts the pulse event.
  /// </summary>
  private void OnPulseComplete()
  {
    // Hide and reset for next pulse
    line.gameObject.SetActive(false);
    line.anchoredPosition = startPos;

    // Broadcast that this stimulus pulsed
    OnStimulusPulsed?.Invoke(stimulusIndex);
  }
}
