using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
public class SceneStreamingConfig : ScriptableObject
{
  [Header("Scenes")]
  public List<AssetReference> scenesToLoad;
  public List<AssetReference> scenesToUnload;
}
