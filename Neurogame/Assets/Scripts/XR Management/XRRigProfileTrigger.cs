using UnityEngine;

public class XRRigProfileTrigger : MonoBehaviour
{
  public XRRigProfile profile;
  public Transform spawnPoint;
  public Transform constraintSource;
  public Vector3 constraintOffset;

  private XRRigController rigController;

  void Start()
  {
    rigController = XRRigController.Instance;
  }

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;

    rigController = rigController != null ? rigController : XRRigController.Instance;

    if (rigController == null || profile == null) return;

    rigController.ApplyProfileSettings(profile);

    if (spawnPoint != null)
    {
      rigController.ApplyPositionAndRotation(spawnPoint);
    }

    if (constraintSource != null)
    {
      rigController.ApplyConstraints(constraintSource, constraintOffset);
    }
  }
}