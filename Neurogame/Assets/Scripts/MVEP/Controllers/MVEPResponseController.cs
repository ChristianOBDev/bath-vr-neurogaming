using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the canoe response behavior based on player input and game phase.
/// 
/// Phase Behaviors:
/// - NoControl: Lane changes only occur on pulse complete, always targeting the power-up lane.
/// - HalfControl: Lane changes can occur on stimulus pulse (if it matches power-up lane AND player inputs) 
///   or on pulse complete (if player inputs any lane). One change per chunk.
/// - FullControl: Lane changes can occur on stimulus pulse (for any lane player inputs) 
///   or on pulse complete regardless of input. One change per chunk.
/// </summary>
public class MVEPResponseController : MonoBehaviour
{
  // Constants
  private const int STIMULUS_COUNT = 5;
  private const int CENTER_LANE = 2;

  // References
  [SerializeField] private CanoeController canoeController;

  // Configuration
  private MVEPGamePhase phase;
  private float p300Interval;
  private float p300min, p300max;
  private float[] timers = new float[STIMULUS_COUNT];
  private bool[] timerActive = new bool[STIMULUS_COUNT];
  private bool[] laneInputReceived = new bool[STIMULUS_COUNT];
  private bool changedLanesThisChunk;
  private int targetLane = CENTER_LANE;

  void OnEnable()
  {
    MVEPInputManager.OnLaneInput += HandleLaneInput;
    MVEPGameEvents.OnChunkActivated += GetActiveChunk;
    MVEPStimulus.OnStimulusPulsed += HandleStimulusPulse;
    MVEPStimuliController.PulseComplete += HandlePulseComplete;
    MVEPGameEvents.OnPhaseChanged += (newPhase) => phase = newPhase;
  }

  void OnDisable()
  {
    MVEPInputManager.OnLaneInput -= HandleLaneInput;
    MVEPGameEvents.OnChunkActivated -= GetActiveChunk;
    MVEPStimulus.OnStimulusPulsed -= HandleStimulusPulse;
    MVEPStimuliController.PulseComplete -= HandlePulseComplete;
    MVEPGameEvents.OnPhaseChanged -= (newPhase) => phase = newPhase;
  }

  /// <summary>
  /// Initializes phase and timing configuration from game settings.
  /// </summary>
  void Start()
  {
    phase = MVEPGameManager.Instance.CurrentPhase;
    p300Interval = MVEPGameManager.Instance.timingConfig.P300Interval;
    p300min = MVEPGameManager.Instance.timingConfig.P300Range.x;
    p300max = MVEPGameManager.Instance.timingConfig.P300Range.y;
  }

  /// <summary>
  /// Handles chunk activation - resets state and updates target lane.
  /// </summary>
  void GetActiveChunk(Chunk chunk)
  {
    ResetChunkState();

    if (IsValidLaneIndex(chunk.PowerUpLane, STIMULUS_COUNT))
    {
      targetLane = chunk.PowerUpLane;
    }
  }

  /// <summary>
  /// Resets all state related to the current chunk.
  /// </summary>
  private void ResetChunkState()
  {
    changedLanesThisChunk = false;
    Array.Clear(timers, 0, timers.Length);
    Array.Clear(timerActive, 0, timerActive.Length);
    Array.Clear(laneInputReceived, 0, laneInputReceived.Length);
  }

  /// <summary>
  /// Handles player lane input, triggering lane changes based on phase and timing.
  /// </summary>
  void HandleLaneInput(int laneIndex)
  {
    if (!IsValidLaneIndex(laneIndex, STIMULUS_COUNT) || changedLanesThisChunk) return;

    laneInputReceived[laneIndex] = true;

    switch (phase)
    {
      case MVEPGamePhase.NoControl:
        // Lane changes handled on pulse complete
        break;
      case MVEPGamePhase.HalfControl:
        AttemptHalfControlLaneChange(laneIndex);
        break;
      case MVEPGamePhase.FullControl:
        AttemptFullControlLaneChange(laneIndex);
        break;
    }
  }

  /// <summary>
  /// Attempts a lane change in Half Control phase (target lane + timer active).
  /// </summary>
  private void AttemptHalfControlLaneChange(int laneIndex)
  {

    if (laneIndex == targetLane && IsTimerValid(laneIndex))
    {
      canoeController.ChangeLanes();
      changedLanesThisChunk = true;
    }
  }

  /// <summary>
  /// Attempts a lane change in Full Control phase (any lane with timer active).
  /// </summary>
  private void AttemptFullControlLaneChange(int laneIndex)
  {
    if (IsTimerValid(laneIndex))
    {

      canoeController.SetTargetLane(laneIndex);
      canoeController.ChangeLanes();
      changedLanesThisChunk = true;
    }
  }

  /// <summary>
  /// Handles stimulus pulse - activates P300 timer window based on phase.
  /// </summary>
  void HandleStimulusPulse(int stimulusIndex)
  {
    if (changedLanesThisChunk || !IsValidLaneIndex(stimulusIndex, STIMULUS_COUNT)) return;

    switch (phase)
    {
      case MVEPGamePhase.NoControl:
        // Lane changes handled on pulse complete
        break;
      case MVEPGamePhase.HalfControl:
        if (stimulusIndex == targetLane)
          timerActive[stimulusIndex] = true;
        break;
      case MVEPGamePhase.FullControl:
        timerActive[stimulusIndex] = true;
        break;
    }
  }

  /// <summary>
  /// Handles pulse complete - automated lane changes based on phase.
  /// </summary>
  void HandlePulseComplete()
  {
    if (changedLanesThisChunk)
      return;

    switch (phase)
    {
      case MVEPGamePhase.NoControl:
      case MVEPGamePhase.FullControl:
        canoeController.ChangeLanes();
        changedLanesThisChunk = true;
        break;
      case MVEPGamePhase.HalfControl:
        if (laneInputReceived.Contains(true))
        {
          canoeController.ChangeLanes();
          changedLanesThisChunk = true;
        }
        break;
    }
  }

  /// <summary>
  /// Updates P300 timers each frame, deactivating them after the interval expires.
  /// </summary>
  void Update()
  {
    UpdateP300Timers();
  }

  /// <summary>
  /// Updates all active P300 interval timers.
  /// </summary>
  private void UpdateP300Timers()
  {
    for (int i = 0; i < timers.Length; i++)
    {
      if (!timerActive[i])
        continue;

      timers[i] += Time.deltaTime;
      if (timers[i] > p300max) // Deactivate timer after maximum interval
      {
        timerActive[i] = false;
        timers[i] = 0f;
      }
    }
  }

  /// <summary>
  /// Validates if a lane index is within valid range.
  /// </summary>
  private bool IsValidLaneIndex(int laneIndex, int maxLanes)
  {
    return laneIndex >= 0 && laneIndex < maxLanes;
  }

  /// <summary>
  /// Checks if the timer for a given stimulus index is active and within the valid P300 response window.
  /// </summary>
  private bool IsTimerValid(int stimulusIndex)
  {
    return timerActive[stimulusIndex] && timers[stimulusIndex] >= p300min && timers[stimulusIndex] <= p300max;
  }
}
