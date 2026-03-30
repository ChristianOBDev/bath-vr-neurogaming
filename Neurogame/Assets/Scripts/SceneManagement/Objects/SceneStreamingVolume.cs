using UnityEngine;

public class SceneStreamingVolume : MonoBehaviour
{
  [SerializeField] private SceneStreamingConfig config;

  [SerializeField] private int loadSignal;
  [SerializeField] private int unloadSignal;

  private void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestLoad(config);
    if (config.scenesToUnload.Count > 0)
      SceneStreamingManager.Instance.RequestUnload(config);

    UDPManager.Instance.Send(loadSignal);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestUnloadOnExit(config);

    UDPManager.Instance.Send(unloadSignal);
  }
}
