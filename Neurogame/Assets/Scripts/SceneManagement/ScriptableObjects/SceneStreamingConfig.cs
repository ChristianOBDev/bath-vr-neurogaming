using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SceneStreamingConfig", menuName = "ScriptableObjects/SceneStreamingConfig", order = 1)]
public class SceneStreamingConfig : ScriptableObject
{
  [Header("Scenes")]
  public List<AssetReference> scenesToLoad;
  public List<AssetReference> scenesToUnload;
  public int loadSignal;
  public int unloadSignal;
  [HideInInspector] public int exitPassiveSignal = 39;

  [HideInInspector] public int enterPassiveSignal = 30;

}
