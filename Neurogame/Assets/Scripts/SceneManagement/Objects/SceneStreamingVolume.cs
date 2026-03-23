using UnityEngine;

public enum StreamingRequestType
{
  Load,
  Unload
}

public class SceneStreamingVolume : MonoBehaviour
{
  [SerializeField] private SceneStreamingConfig config;

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("Player entered streaming volume: " + gameObject.name);
    if (!other.CompareTag("Player"))
      return;

    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestLoad(config);
    if (config.scenesToUnload.Count > 0)
      SceneStreamingManager.Instance.RequestUnload(config);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    Debug.Log("Player exited streaming volume: " + gameObject.name);
    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestUnloadOnExit(config);
  }
}
