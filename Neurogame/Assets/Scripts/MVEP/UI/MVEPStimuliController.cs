using System.Collections;
using System;
using UnityEngine;

/// <summary>
/// Controls the MVEP stimulus presentation sequence.
/// Manages pulse timing, stimulus ordering, and P300 response window.
/// Stimuli are presented in randomized order to prevent learned patterns.
/// </summary>
public class MVEPStimuliController : MonoBehaviour
{
  // Constants
  private const float DEFAULT_PULSE_DELAY = 0f;

  // Configuration - Timing
  private float pulseInterval;
  private float p300Interval;

  // Component References
  [SerializeField] private MVEPStimulus[] mvepStimuli;

  // Events
  /// <summary>
  /// Fired when a complete pulse sequence and P300 interval has finished.
  /// Signals that the player has had their response window.
  /// </summary>
  public static Action PulseComplete;

  /// <summary>
  /// Initializes timing configuration from game settings.
  /// </summary>
  private void Awake()
  {
    pulseInterval = MVEPGameSettings.Instance.timingConfig.PulseInterval;
    p300Interval = MVEPGameSettings.Instance.timingConfig.P300Interval;

    ValidateStimuliSetup();
  }

  /// <summary>
  /// Validates that stimuli are properly configured.
  /// </summary>
  private void ValidateStimuliSetup()
  {
    if (mvepStimuli == null || mvepStimuli.Length == 0)
    {
      Debug.LogError("MVEPStimuliController: No stimuli assigned in inspector!");
    }
  }

  /// <summary>
  /// Initiates a pulse sequence with optional delay before starting.
  /// </summary>
  /// <param name="delay">Delay in seconds before the pulse sequence starts. Default is 0.</param>
  public void Pulse(float delay = DEFAULT_PULSE_DELAY)
  {
    if (mvepStimuli == null || mvepStimuli.Length == 0)
    {
      Debug.LogError("MVEPStimuliController: Cannot pulse without stimuli configured!");
      return;
    }

    StartCoroutine(ExecutePulseSequence(delay));
  }

  /// <summary>
  /// Executes the complete pulse sequence including delay, stimulus presentation, and P300 wait.
  /// Stimuli are presented in randomized order and spaced by pulseInterval.
  /// After all stimuli, waits for P300 response window before completing.
  /// </summary>
  /// <param name="delay">Initial delay in seconds before starting stimuli presentation.</param>
  /// <returns>Coroutine enumerator.</returns>
  private IEnumerator ExecutePulseSequence(float delay)
  {
    // Wait for initial delay if specified
    if (delay > 0f)
    {
      yield return new WaitForSeconds(delay);
    }

    // Get randomized stimulus order
    MVEPStimulus[] shuffledStimuli = GetShuffledStimuli();

    // Present each stimulus with spacing
    foreach (var stimulus in shuffledStimuli)
    {
      stimulus.Pulse();
      yield return new WaitForSeconds(pulseInterval);
    }

    // Wait for P300 response window
    yield return new WaitForSeconds(p300Interval);

    // Signal sequence complete
    PulseComplete?.Invoke();
  }

  /// <summary>
  /// Creates a shuffled copy of the stimuli array using the Fisher-Yates shuffle algorithm.
  /// Ensures each stimulus appears once in random order.
  /// </summary>
  /// <returns>A new array with stimuli in randomized order.</returns>
  private MVEPStimulus[] GetShuffledStimuli()
  {
    MVEPStimulus[] shuffled = new MVEPStimulus[mvepStimuli.Length];
    mvepStimuli.CopyTo(shuffled, 0);

    // Fisher-Yates shuffle algorithm
    for (int i = 0; i < shuffled.Length; i++)
    {
      MVEPStimulus temp = shuffled[i];
      int randomIndex = UnityEngine.Random.Range(i, shuffled.Length);

      // Swap
      shuffled[i] = shuffled[randomIndex];
      shuffled[randomIndex] = temp;
    }

    return shuffled;
  }
}
