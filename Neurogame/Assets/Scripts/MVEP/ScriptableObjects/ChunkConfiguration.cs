using UnityEngine;

[CreateAssetMenu(fileName = "ChunkConfig", menuName = "MVEP/ChunkConfig", order = 1)]
public class ChunkConfiguration : ScriptableObject
{
  [Header("Prefab")]
  public Chunk chunkPrefab;
  public PowerUp powerUpPrefab;
  public Obstacle obstaclePrefab;
}
