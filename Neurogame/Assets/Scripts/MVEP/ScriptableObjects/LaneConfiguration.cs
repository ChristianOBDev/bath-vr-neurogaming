using UnityEngine;

[CreateAssetMenu(fileName = "LaneConfig", menuName = "ScriptableObjects/LaneConfig", order = 1)]
public class LaneConfiguration : ScriptableObject
{
  public int laneCount = 5;
  public int laneWidth = 2;

  public float GetLaneXPosition(int laneIndex)
  {
    float centerOffset = (laneCount - 1) * laneWidth / 2f;
    return (laneIndex * laneWidth) - centerOffset;
  }
}
