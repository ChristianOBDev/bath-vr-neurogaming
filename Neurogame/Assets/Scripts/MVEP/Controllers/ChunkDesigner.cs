using UnityEngine;

public class ChunkDesigner : MonoBehaviour
{
  private int lastObstacleLane = -1;
  private int lastPowerUpLane = -1;

  public void DesignChunk(
      Chunk chunk,
      ObjectPool<Obstacle> obstaclePool,
      ObjectPool<PowerUp> powerUpPool)
  {
    ClearChunk(chunk);

    int laneCount = chunk.LaneAnchors.Length;

    int obstacleLane = -1;
    int powerUpLane = -1;
    switch (MVEPGameSettings.Instance.CurrentPhase)
    {
      case Phase.NoControl:
      case Phase.HalfControl:
      case Phase.FullControl:
        if (lastPowerUpLane >= 0 && lastPowerUpLane < laneCount)
        {
          obstacleLane = lastPowerUpLane;
        }
        else
        {
          obstacleLane = Random.Range(0, laneCount);
        }

        do
        {
          powerUpLane = Random.Range(0, laneCount);
        }
        while (powerUpLane == lastObstacleLane || powerUpLane == obstacleLane);
        break;
    }

    chunk.SetLanes(obstacleLane, powerUpLane);

    lastObstacleLane = obstacleLane;
    lastPowerUpLane = powerUpLane;

    Spawn(obstaclePool.Get(), chunk.LaneAnchors[obstacleLane]);
    Spawn(powerUpPool.Get(), chunk.LaneAnchors[powerUpLane]);

    foreach (var riverBank in chunk.RiverBanks)
    {
      riverBank.Randomize();
    }
  }

  private void Spawn(Component obj, Transform anchor)
  {
    obj.transform.SetParent(anchor);
    obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
  }

  private void ClearChunk(Chunk chunk)
  {
    foreach (Transform lane in chunk.LaneAnchors)
    {
      for (int i = lane.childCount - 1; i >= 0; i--)
      {
        if (lane.GetChild(i).TryGetComponent(out IPoolable<Obstacle> pooledObstacle))
        {
          pooledObstacle.ReturnToPool();
        }
        else if (lane.GetChild(i).TryGetComponent(out IPoolable<PowerUp> pooledPowerUp))
        {
          pooledPowerUp.ReturnToPool();
        }
      }
    }
  }
}
/// Use this for NoControl phase if desired
// obstacleLane = Random.Range(0, laneCount);
// do
// {
//   powerUpLane = Random.Range(0, laneCount);
// }
// while (powerUpLane == obstacleLane);
// break;
