using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneStreamingConfig", menuName = "ScriptableObjects/SceneStreamingConfig", order = 1)]
public class SceneStreamingConfig : ScriptableObject
{
  [Header("Scenes")]
  public List<AssetReference> scenesToLoad;
  public List<AssetReference> scenesToUnload;
}
