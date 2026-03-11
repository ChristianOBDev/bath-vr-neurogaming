using UnityEngine;

public enum MVEPGamePhase
{
  NoControl,
  HalfControl,
  FullControl
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
  [SerializeField] private MVEPGamePhase currentPhase;
  public MVEPGamePhase CurrentPhase { get { return currentPhase; } private set { currentPhase = value; } }
  private int TrialsCompleted { get; set; }

  #endregion

  #region References

  [Header("Component References")]
  public ChunkManager chunkManager;
  public XRRigProfileTrigger rigProfileTrigger;

  #endregion

  #region Unity callbacks

  private void OnEnable()
  {
    if (rigProfileTrigger != null)
      rigProfileTrigger.onRigProfileApplied += SpawnChunks;

    MVEPGameEvents.OnChunkPassed += HandleChunkPassed;
  }

  private void OnDisable()
  {
    if (rigProfileTrigger != null)
      rigProfileTrigger.onRigProfileApplied -= SpawnChunks;

    MVEPGameEvents.OnChunkPassed -= HandleChunkPassed;
  }

  #endregion

  #region Public API

  /// <summary>
  /// fire once the appropriate rig profile has been applied.
  /// </summary>
  public void SpawnChunks() => chunkManager.SpawnInitialChunks();

  public void StartGame() => chunkManager.ActivateGame(true);

  public void PauseGame() => chunkManager.ActivateGame(false);

  public void QuitGame()
  {
    if (rigProfileTrigger != null) rigProfileTrigger.Reset();
    PauseGame();
    chunkManager.ClearChunks();
  }

  /// <summary>
  /// change the current phase and notify listeners.
  /// </summary>
  public void SetPhase(MVEPGamePhase phase)
  {
    CurrentPhase = phase;
    MVEPGameEvents.OnPhaseChanged?.Invoke(phase);
  }

  #endregion

  #region Event handlers

  private void HandleChunkPassed(Chunk chunk)
  {
    if (!chunk.IsValid())
      return;

    TrialsCompleted++;
    if (TrialsCompleted >= gameConfig.Trials)
      PauseGame();
  }

  #endregion
}