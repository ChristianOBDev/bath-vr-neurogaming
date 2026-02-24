using UnityEngine;

[CreateAssetMenu(fileName = "ChunkConfig", menuName = "ScriptableObjects/ChunkConfig", order = 1)]
public class ChunkConfiguration : ScriptableObject
{
  [Header("Chunk Settings")]
  public float chunkLength = 30f;
  public float chunkTraversalTime = 5.3f;

  [Header("Prefab")]
  public Chunk chunkPrefab;
}
