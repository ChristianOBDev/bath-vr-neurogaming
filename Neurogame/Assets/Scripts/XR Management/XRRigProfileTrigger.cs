using System;
using UnityEngine;

public class XRRigProfileTrigger : MonoBehaviour
{
  public XRRigProfile profile;
  public Transform spawnPoint;
  public Transform constraintSource;
  public Vector3 constraintOffset;

  public Transform resetPoint;

  private XRRigController rigController;

  public Action onRigProfileApplied;
  public Action onRigReset;

  void Start()
  {
    rigController = XRRigController.Instance;

    if (profile.inputActions != null)
      profile.inputActions.Disable();
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

    onRigProfileApplied?.Invoke();
  }

  public void Reset()
  {
    if (rigController != null)
    {
      rigController.ResetRig(resetPoint);

      if (profile.inputActions != null)
        profile.inputActions.Disable();

      onRigReset?.Invoke();
    }
  }
}