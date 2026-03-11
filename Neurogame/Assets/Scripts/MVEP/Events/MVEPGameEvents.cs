using System;

/// <summary>
/// Central event system for MVEP game events.
/// Provides static events for various game state changes and interactions.
/// Subscribe to these events to respond to game activity without tight coupling.
/// </summary>
public static class MVEPGameEvents
{
  // ========== Canoe & Interaction Events ==========

  /// <summary>
  /// Fired when the canoe returns to the center lane.
  /// </summary>
  public static Action OnCanoeCentered;

  /// <summary>
  /// Fired when the player collects a power-up.
  /// </summary>
  public static Action OnPowerUpCollected;

  /// <summary>
  /// Fired when the canoe collides with an obstacle.
  /// </summary>
  public static Action OnObstacleHit;

  // ========== Game State Events ==========

  /// <summary>
  /// Fired when the game starts.
  /// </summary>
  public static Action OnGameStarted;

  /// <summary>
  /// Fired when the game is paused.
  /// </summary>
  public static Action OnGamePaused;

  /// <summary>
  /// Fired when the game is resumed.
  /// </summary>
  public static Action OnGameResumed;

  /// <summary>
  /// Fired when the game speed changes. Contains the new speed value.
  /// </summary>
  public static Action<float> OnSpeedChanged;

  /// <summary>
  /// Fired when the player's score is updated. Contains the new score value.
  /// </summary>
  public static Action<int> OnScoreUpdated;

  // ========== Chunk Lifecycle Events ==========

  /// <summary>
  /// Fired when a new chunk becomes the active chunk (the one the player must navigate). Contains the newly activated chunk.
  /// </summary>
  public static Action<Chunk> OnChunkActivated;

  /// <summary>
  /// Fired when a chunk is completely passed and no longer in play. Contains the passed chunk.
  /// </summary>
  public static Action<Chunk> OnChunkPassed;

  /// <summary>
  /// Fired when the game ends.
  /// </summary>
  public static Action OnGameEnded;

  // ========== Game Phase Events ==========
  /// <summary>
  /// Fired when the game phase changes. Contains the new game phase.
  /// </summary>
  public static Action<MVEPGamePhase> OnPhaseChanged;
}
