using System;
using UnityEngine;

[Serializable]
public enum MVEPGamePhase
{
  NoControl,
  HalfControl,
  FullControl
}

public enum MVEPGameState
{
  NotStarted,
  Running,
  Paused,
  Ended,
  Quit
}

/// <summary>
/// central game manager – holds configuration, current phase and routes
/// high–level game events to the chunk manager, rig trigger, etc.
/// </summary>
public class MVEPGameManager : Singleton<MVEPGameManager>
{
  #region Configs

  [Header("Configs")]
  public MVEPGameConfiguration gameConfig;
  public LaneConfiguration laneConfig;
  public ChunkConfiguration chunkConfig;
  public CanoeScoreConfig scoreConfig;
  public MVEPTimingConfiguration timingConfig;

  #endregion

  #region State

  [Header("Game State")]
  private MVEPGameState currentState = MVEPGameState.NotStarted;
  public MVEPGameState CurrentState { get { return currentState; } private set { currentState = value; } }
  [SerializeField] private MVEPGamePhase currentPhase;
  public MVEPGamePhase CurrentPhase { get { return currentPhase; } private set { currentPhase = value; } }
  private int TrialsCompleted { get; set; }

  #endregion

  #region References

  [Header("Component References")]
  public ChunkManager chunkManager;
  public MVEPScoreManager scoreManager;
  public MVEPInfoPanel infoPanel;
  public XRRigProfileTrigger rigProfileTrigger;
  public CountdownTimer countdownTimer;
  public AudioSource riverSFXSource;
  public AudioSource musicSource;

  #endregion

  #region Unity callbacks

  private void OnEnable()
  {
    MVEPGameEvents.OnChunkDeactivated += HandleChunkDeactivated;
    rigProfileTrigger.onRigProfileApplied += OnRigApplied;
  }

  private void OnDisable()
  {
    MVEPGameEvents.OnChunkDeactivated -= HandleChunkDeactivated;
  }

  #endregion

  #region Public API

  public void OnRigApplied()
  {
    infoPanel.ShowStartScreen();
    if (musicSource != null) musicSource.Play();
    if (riverSFXSource != null) riverSFXSource.Play();
  }

  public void StartGame()
  {
    TrialsCompleted = 0;
    infoPanel.HideAll();
    countdownTimer.StartCountdown(() =>
    {
      MVEPGameEvents.OnGameStarted?.Invoke();
      currentState = MVEPGameState.Running;
    });
  }

  public void PauseGame()
  {
    MVEPGameEvents.OnGamePaused?.Invoke();
    currentState = MVEPGameState.Paused;
  }

  public void ResumeGame()
  {
    countdownTimer.StartCountdown(() =>
    {
      currentState = MVEPGameState.Running;
      MVEPGameEvents.OnGameResumed?.Invoke();
    });
  }

  public void EndGame()
  {
    currentState = MVEPGameState.Ended;
    int[] scoreBreakdown = scoreManager.GetScoreBreakdown();
    infoPanel.ShowEndScreen(scoreBreakdown[0], scoreBreakdown[1], scoreBreakdown[2]);
    MVEPGameEvents.OnGameEnded?.Invoke();
  }

  public void QuitGame()
  {
    if (rigProfileTrigger != null) rigProfileTrigger.Reset();
    currentState = MVEPGameState.Quit;
    MVEPGameEvents.OnGameEnded?.Invoke();
    if (musicSource != null) musicSource.Stop();
    if (riverSFXSource != null) riverSFXSource.Stop();
  }

  /// <summary>
  /// change the current phase and notify listeners.
  /// </summary>
  public void SetPhase(int phase)
  {
    CurrentPhase = (MVEPGamePhase)phase;
    MVEPGameEvents.OnPhaseChanged?.Invoke(CurrentPhase);
  }

  #endregion

  #region Event handlers

  private void HandleChunkDeactivated(Chunk chunk)
  {
    if (!chunk.IsValid())
      return;

    TrialsCompleted++;
    if (TrialsCompleted >= gameConfig.Trials)
      EndGame();
  }

  #endregion
}