using UnityEngine;

public class ChunkDesigner : MonoBehaviour
{
  // Constants
  private const int NO_LANE = -1;

  // State
  private int lastObstacleLane = NO_LANE;
  private int lastPowerUpLane = NO_LANE;

  /// <summary>
  /// Designs and populates a chunk with obstacles and power-ups in different lanes.
  /// </summary>
  /// <param name="chunk">The chunk to design.</param>
  /// <param name="obstaclePool">Object pool for obstacles.</param>
  /// <param name="powerUpPool">Object pool for power-ups.</param>
  public void DesignChunk(
      Chunk chunk,
      ObjectPool<Obstacle> obstaclePool,
      ObjectPool<PowerUp> powerUpPool)
  {
    if (chunk == null || obstaclePool == null || powerUpPool == null)
      return;

    ClearChunk(chunk);

    int laneCount = chunk.LaneAnchors.Length;

    // Determine lanes for obstacle and power-up
    int obstacleLane = DetermineObstacleLane(laneCount);
    int powerUpLane = DeterminePowerUpLane(laneCount, obstacleLane);

    // Store for next chunk and spawn objects
    chunk.SetLanes(obstacleLane, powerUpLane);
    lastObstacleLane = obstacleLane;
    lastPowerUpLane = powerUpLane;

    Spawn(obstaclePool.Get(), chunk.LaneAnchors[obstacleLane]);
    Spawn(powerUpPool.Get(), chunk.LaneAnchors[powerUpLane]);
  }

  /// <summary>
  /// Determines which lane should contain the obstacle.
  /// Prefers the lane where the power-up was placed in the previous chunk.
  /// </summary>
  private int DetermineObstacleLane(int laneCount)
  {
    if (IsValidLane(lastPowerUpLane, laneCount))
    {
      return lastPowerUpLane;
    }
    return Random.Range(0, laneCount);
  }

  /// <summary>
  /// Determines which lane should contain the power-up.
  /// Ensures the power-up is not in the same lane as the obstacle or previous obstacle.
  /// </summary>
  private int DeterminePowerUpLane(int laneCount, int obstacleLane)
  {
    int powerUpLane;
    do
    {
      powerUpLane = Random.Range(0, laneCount);
    }
    while (powerUpLane == lastObstacleLane || powerUpLane == obstacleLane);
    return powerUpLane;
  }

  /// <summary>
  /// Checks if a lane index is valid.
  /// </summary>
  private bool IsValidLane(int lane, int laneCount)
  {
    return lane >= 0 && lane < laneCount;
  }

  /// <summary>
  /// Spawns an object at the specified anchor position.
  /// </summary>
  private void Spawn(Component obj, Transform anchor)
  {
    obj.transform.SetParent(anchor);
    obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
  }

  /// <summary>
  /// Clears all pooled objects from a chunk's lanes.
  /// </summary>
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
