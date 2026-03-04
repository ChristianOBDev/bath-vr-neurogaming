using UnityEngine;

/// <summary>
/// Configuration for MVEP game timing parameters.
/// Controls timing for chunks, canoe movement, stimuli presentation, and overall game flow.
/// All derived timings are calculated from the base configuration values.
/// </summary>
[CreateAssetMenu(fileName = "MVEPTimingConfig", menuName = "MVEP/MVEPTimingConfig", order = 1)]
public class MVEPTimingConfiguration : ScriptableObject
{
  // ========== CHUNK TIMING ==========

  /// <summary>
  /// Length of each chunk in world units.
  /// Determines the visual size of each chunk segment.
  /// </summary>
  [SerializeField]
  [Tooltip("Length of each chunk in units")]
  private float chunkLength = 30f;
  public float ChunkLength => chunkLength;

  /// <summary>
  /// Time duration for the player to traverse past a chunk after interaction is complete.
  /// This is the "coasting" time before the next chunk becomes active.
  /// </summary>
  [SerializeField]
  [Tooltip("Time after player interaction before they pass the chunk gate")]
  private float traversalDuration = 3.3f;
  public float TraversalDuration => traversalDuration;

  // ========== CANOE MOVEMENT TIMING ==========

  /// <summary>
  /// How long it takes the canoe to change lanes after a stimulus response.
  /// </summary>
  [SerializeField]
  [Tooltip("Time to complete a lane change animation")]
  private float laneChangeDuration = 1f;
  public float LaneChangeDuration => laneChangeDuration;

  /// <summary>
  /// Time to reset the canoe back to the center lane.
  /// </summary>
  [SerializeField]
  [Tooltip("Time to reset canoe to center lane")]
  private float laneResetDuration = 1.0f;
  public float LaneResetDuration => laneResetDuration;

  // ========== STIMULI TIMING ==========

  /// <summary>
  /// Number of stimuli in each stimulus wave.
  /// </summary>
  [SerializeField]
  [Tooltip("Number of visual stimuli in a wave")]
  private int numStimuli = 5;
  public int NumStimuli => numStimuli;

  /// <summary>
  /// Time before stimuli presentation to show the arrow indicator to the player.
  /// Allows the player to anticipate the upcoming stimuli.
  /// </summary>
  [SerializeField]
  [Tooltip("Time to show arrow before stimuli starts")]
  private float arrowPreactivationDuration = 1.2f;
  public float ArrowPreactivationDuration => arrowPreactivationDuration;

  /// <summary>
  /// Duration each individual stimulus is presented.
  /// </summary>
  [SerializeField]
  [Tooltip("How long each stimulus flashes")]
  private float pulseDuration = 0.14f;
  public float PulseDuration => pulseDuration;

  /// <summary>
  /// Time between stimulus pulses (gap between presentations).
  /// </summary>
  [SerializeField]
  [Tooltip("Time between stimulus pulses")]
  private float pulseOffset = 0.06f;
  public float PulseOffset => pulseOffset;

  /// <summary>
  /// Time window after stimulus pulse for player to register a response (P300).
  /// </summary>
  [SerializeField]
  [Tooltip("Time window after pulse for response registration")]
  private float p300Interval = 0.3f;
  public float P300Interval => p300Interval;

  // ========== DERIVED TIMING CALCULATIONS ==========

  /// <summary>
  /// Total time for one stimulus pulse cycle (pulse + gap).
  /// </summary>
  public float PulseInterval => pulseDuration + pulseOffset;

  /// <summary>
  /// Total time to present all stimuli in a wave.
  /// </summary>
  public float WaveDuration => numStimuli * PulseInterval;

  /// <summary>
  /// Total time for stimulus presentation and P300 response window.
  /// </summary>
  public float StimuliDuration => WaveDuration + p300Interval;

  /// <summary>
  /// Total time for canoe lateral movement (reset + change).
  /// </summary>
  public float LateralMotionDuration => laneResetDuration + laneChangeDuration;

  /// <summary>
  /// Total time for one complete chunk interaction cycle.
  /// Includes: lateral motion + stimuli + P300 response + traversal.
  /// </summary>
  public float TotalTime => LateralMotionDuration + StimuliDuration + traversalDuration;
}
