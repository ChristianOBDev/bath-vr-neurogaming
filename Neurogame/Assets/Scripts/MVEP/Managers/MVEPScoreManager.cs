using UnityEngine;

public class MVEPScoreManager : MonoBehaviour
{
  public int TotalScore => totalScore;
  [SerializeField] private int totalScore;
  private int chunkScore;
  private int powerUpScore;
  private int obstaclePenalty;

  private int scorePerChunk = 10;
  private int scorePerPowerUp = 50;
  private int scorePenaltyPerObstacle = 30;

  void Start()
  {
    var settings = MVEPGameSettings.Instance;
    scorePerChunk = settings.scoreConfig.scorePerChunk;
    scorePerPowerUp = settings.scoreConfig.scorePerPowerUp;
    scorePenaltyPerObstacle = settings.scoreConfig.scorePenaltyPerObstacle;
  }

  void OnEnable()
  {
    MVEPGameEvents.OnPowerUpCollected += AddPowerUpPoints;
    MVEPGameEvents.OnObstacleHit += SubtractObstaclePoints;
    Chunk.OnChunkPassed += OnChunkPassed;
  }

  void OnDisable()
  {
    MVEPGameEvents.OnPowerUpCollected -= AddPowerUpPoints;
    MVEPGameEvents.OnObstacleHit -= SubtractObstaclePoints;
    Chunk.OnChunkPassed -= OnChunkPassed;
  }

  private void OnChunkPassed(Chunk chunk)
  {
    if (chunk.ObstacleLane < 0) return;

    chunkScore += scorePerChunk;
    RecalculateScore();
  }

  public void AddPowerUpPoints()
  {
    powerUpScore += scorePerPowerUp;
    RecalculateScore();
  }

  public void SubtractObstaclePoints()
  {
    obstaclePenalty += scorePenaltyPerObstacle;
    RecalculateScore();
  }

  private void RecalculateScore()
  {
    totalScore = powerUpScore + chunkScore - obstaclePenalty;
    MVEPGameEvents.OnScoreUpdated?.Invoke(totalScore);
  }
}
