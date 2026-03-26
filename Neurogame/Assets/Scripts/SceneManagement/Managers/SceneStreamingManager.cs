using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneStreamingManager : PersistentSingleton<SceneStreamingManager>
{
  private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> loadedScenes = new();

  // -------- PUBLIC METHODS --------

  public void RequestLoad(SceneStreamingConfig config)
  {
    foreach (var sceneRef in config.scenesToLoad)
    {
      TryLoadScene(sceneRef);
    }

  }

  public void RequestUnload(SceneStreamingConfig config)
  {
    foreach (var sceneRef in config.scenesToUnload)
    {
      TryUnloadScene(sceneRef);
    }
  }

  public void RequestUnloadOnExit(SceneStreamingConfig config)
  {
    foreach (var sceneRef in config.scenesToLoad)
    {
      TryUnloadScene(sceneRef);
    }
  }

  // -------- PRIVATE METHODS --------
  private void TryLoadScene(AssetReference sceneRef)
  {
    string key = sceneRef.RuntimeKey.ToString();
    if (loadedScenes.ContainsKey(key))
    {
      return;
    }

    StartCoroutine(LoadScene(sceneRef, key));
  }

  private void TryUnloadScene(AssetReference sceneRef)
  {
    string key = sceneRef.RuntimeKey.ToString();

    if (!loadedScenes.ContainsKey(key))
      return;

    StartCoroutine(UnloadScene(key));
  }

  // -------- COROUTINES --------

  private IEnumerator LoadScene(AssetReference sceneRef, string key)
  {
    var handle = Addressables.LoadSceneAsync(
            sceneRef,
            LoadSceneMode.Additive,
            activateOnLoad: true
        );

    yield return handle;

    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
      loadedScenes.Add(key, handle);
    }
    else
    {
      Debug.LogError($"Failed to load scene: {key}");
    }
  }

  private IEnumerator UnloadScene(string key)
  {
    var handle = loadedScenes[key];

    var unloadHandle = Addressables.UnloadSceneAsync(handle);
    yield return unloadHandle;

    loadedScenes.Remove(key);
  }
}
