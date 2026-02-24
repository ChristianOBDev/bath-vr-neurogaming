using UnityEngine;

[CreateAssetMenu(fileName = "CanoeScoreConfig", menuName = "MVEP/Canoe Score Configuration", order = 1)]
public class CanoeScoreConfig : ScriptableObject
{
  public int scorePerChunk = 10;
  public int scorePerPowerUp = 50;
  public int scorePenaltyPerObstacle = 30;
}
