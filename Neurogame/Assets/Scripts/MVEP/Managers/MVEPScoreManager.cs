using UnityEngine;

/// <summary>
/// Manages player score throughout the game.
/// Tracks scores from chunk completion, power-ups collected, and obstacle penalties.
/// Calculates and broadcasts the total score to all listeners.
/// </summary>
public class MVEPScoreManager : MonoBehaviour
{
  // Default Score Values
  private const int DEFAULT_SCORE_PER_CHUNK = 10;
  private const int DEFAULT_SCORE_PER_POWERUP = 50;
  private const int DEFAULT_SCORE_PENALTY_PER_OBSTACLE = 30;

  // Configuration
  private int scorePerChunk;
  private int scorePerPowerUp;
  private int scorePenaltyPerObstacle;

  // Score Tracking
  private int totalScore;
  private int chunkScore;
  private int powerUpScore;
  private int obstaclePenalty;

  /// <summary>
  /// Gets the current total score.
  /// </summary>
  public int TotalScore => totalScore;

  /// <summary>
  /// Initializes score configuration from game settings.
  /// </summary>
  private void Start()
  {
    var settings = MVEPGameSettings.Instance;
    scorePerChunk = settings.scoreConfig.scorePerChunk;
    scorePerPowerUp = settings.scoreConfig.scorePerPowerUp;
    scorePenaltyPerObstacle = settings.scoreConfig.scorePenaltyPerObstacle;
  }

  /// <summary>
  /// Subscribes to game events that affect scoring.
  /// </summary>
  private void OnEnable()
  {
    MVEPGameEvents.OnPowerUpCollected += HandlePowerUpCollected;
    MVEPGameEvents.OnObstacleHit += HandleObstacleHit;
    MVEPGameEvents.OnChunkPassed += HandleChunkPassed;
  }

  /// <summary>
  /// Unsubscribes from game events.
  /// </summary>
  private void OnDisable()
  {
    MVEPGameEvents.OnPowerUpCollected -= HandlePowerUpCollected;
    MVEPGameEvents.OnObstacleHit -= HandleObstacleHit;
    MVEPGameEvents.OnChunkPassed -= HandleChunkPassed;
  }

  /// <summary>
  /// Handles chunk passed event, awarding points for safe passage.
  /// Only awards points if the chunk contained an obstacle (had valid ObstacleLane).
  /// </summary>
  /// <param name="chunk">The chunk that was passed.</param>
  private void HandleChunkPassed(Chunk chunk)
  {
    // Only award chunk points if chunk had an obstacle (ObstacleLane >= 0)
    if (chunk.ObstacleLane >= 0)
    {
      chunkScore += scorePerChunk;
      RecalculateScore();
    }
  }

  /// <summary>
  /// Adds points when the player collects a power-up.
  /// </summary>
  private void HandlePowerUpCollected()
  {
    powerUpScore += scorePerPowerUp;
    RecalculateScore();
  }

  /// <summary>
  /// Subtracts points when the player hits an obstacle.
  /// </summary>
  private void HandleObstacleHit()
  {
    obstaclePenalty += scorePenaltyPerObstacle;
    RecalculateScore();
  }

  /// <summary>
  /// Recalculates total score and broadcasts the update.
  /// Total = powerUpScore + chunkScore - obstaclePenalty
  /// </summary>
  private void RecalculateScore()
  {
    totalScore = powerUpScore + chunkScore - obstaclePenalty;
    MVEPGameEvents.OnScoreUpdated?.Invoke(totalScore);
  }
}
