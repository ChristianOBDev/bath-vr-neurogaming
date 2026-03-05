using System.Linq;
using UnityEngine;

public class MVEPResponseController : MonoBehaviour
{
  [SerializeField] private CanoeController canoeController;
  private Phase phase;

  private float[] timers = new float[5];
  private bool[] timerActive = new bool[5];
  private bool[] laneInputReceived = new bool[5];
  private bool changedLanesThisChunk = false; // Flag to track if a lane change has already occurred in the current chunk
  private float p300Interval = 0.3f; // Time window after stimulus pulse to allow lane changes

  private int targetLane = 2;

  void Awake()
  {
    phase = MVEPGameSettings.Instance.CurrentPhase;
    p300Interval = MVEPGameSettings.Instance.mvepConfig.p300Interval;
  }

  void OnEnable()
  {
    MVEPInputManager.OnLaneInput += HandleLaneInput;
    Chunk.OnChunkActivated += GetActiveChunk;
    MVEPStimulus.OnStimulusPulsed += HandleStimulusPulse;
    MVEPStimuliController.PulseComplete += HandlePulseComplete;
  }

  void OnDisable()
  {
    MVEPInputManager.OnLaneInput -= HandleLaneInput;
    Chunk.OnChunkActivated -= GetActiveChunk;
    MVEPStimulus.OnStimulusPulsed -= HandleStimulusPulse;
    MVEPStimuliController.PulseComplete -= HandlePulseComplete;
  }

  void GetActiveChunk(Chunk chunk)
  {
    changedLanesThisChunk = false;
    timers = new float[5];
    timerActive = new bool[5];
    laneInputReceived = new bool[5];

    if (chunk.PowerUpLane >= 0 && chunk.PowerUpLane < timers.Length)
    {
      targetLane = chunk.PowerUpLane;
    }
  }

  void HandleLaneInput(int laneIndex)
  {
    if (laneIndex < 0 || laneIndex >= laneInputReceived.Length) return;

    laneInputReceived[laneIndex] = true;
    if (changedLanesThisChunk) return;

    switch (phase)
    {
      case Phase.NoControl:
        // Do nothing, lane changes are handled on pulse complete
        break;
      case Phase.HalfControl:
        if (laneIndex == targetLane && timerActive[laneIndex])
        {
          canoeController.ChangeLanes();
          changedLanesThisChunk = true;
        }
        break;
      case Phase.FullControl:
        if (timerActive[laneIndex])
        {
          canoeController.SetTargetLane(laneIndex);
          canoeController.ChangeLanes();
          changedLanesThisChunk = true;
        }
        break;
    }
  }

  void HandleStimulusPulse(int stimulusIndex)
  {
    if (changedLanesThisChunk) return;
    switch (phase)
    {
      case Phase.NoControl:
        // Do nothing, lane changes are handled on pulse complete
        break;
      case Phase.HalfControl:
        if (stimulusIndex == targetLane) timerActive[stimulusIndex] = true;
        break;
      case Phase.FullControl:
        timerActive[stimulusIndex] = true;
        break;
    }
  }

  void HandlePulseComplete()
  {
    if (changedLanesThisChunk) return;
    switch (phase)
    {
      case Phase.NoControl:
        canoeController.ChangeLanes();
        break;
      case Phase.HalfControl:
        if (laneInputReceived.Contains(true))
        {
          canoeController.ChangeLanes();
          changedLanesThisChunk = true;
          return;
        }
        break;
      case Phase.FullControl:
        canoeController.ChangeLanes();
        break;
    }
  }

  void Update()
  {
    for (int i = 0; i < timers.Length; i++)
    {
      if (timerActive[i])
      {
        timers[i] += Time.deltaTime;
        if (timers[i] > p300Interval)
        {
          timerActive[i] = false;
          timers[i] = 0f;
        }
      }
    }
  }
}


/// What should happen in each phase:
/// No Control: Lane changes only occur on pulse complete, and the target lane is always the power-up lane
/// Half Control: Lane changes can occur on stimulus pulse if the stimulus corresponds to the power-up lane, but only if the player has also input a lane change for that lane. Lane changes can also occur on pulse complete if the player input a lane change for any lane, even if it doesn't correspond to the power-up lane. The player can only change lanes once per chunk, so if they change lanes on stimulus pulse they cannot change lanes again on pulse complete, and vice versa.
/// Full Control: Lane changes can occur on stimulus pulse for any stimulus, as long as the player has input a lane change for that lane. Lane changes can also occur on pulse complete regardless of player input. The player can only change lanes once per chunk, so if they change lanes on stimulus pulse they cannot change lanes again on pulse complete, and vice versa.
