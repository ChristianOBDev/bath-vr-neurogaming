using UnityEngine;

public class SceneStreamingVolume : MonoBehaviour
{
  [SerializeField] private SceneStreamingConfig config;

  private void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestLoad(config);
    if (config.scenesToUnload.Count > 0)
      SceneStreamingManager.Instance.RequestUnload(config);

    UDPManager.Instance.Send(config.exitPassiveSignal);
    UDPManager.Instance.Send(config.loadSignal);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    if (config.scenesToLoad.Count > 0)
      SceneStreamingManager.Instance.RequestUnloadOnExit(config);

    UDPManager.Instance.Send(config.unloadSignal);
    UDPManager.Instance.Send(config.enterPassiveSignal);
  }
}
