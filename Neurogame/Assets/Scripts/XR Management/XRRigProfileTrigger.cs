using System;
using UnityEngine;
using UnityEngine.Animations;

public class XRRigProfileTrigger : MonoBehaviour
{
  public XRRigProfile profile;
  public Transform spawnPoint;
  public Transform constraintSource;
  public Vector3 constraintOffset;

  public Transform resetPoint;

  #region 2D PC Settings
  public bool flatScreenPCVersion;
  public Transform pcSpawnPoint;
  public Transform pcResetPoint;
  [SerializeField] private BasicFPCC playerController;
  public bool disableMovement;
  #endregion

  private XRRigController rigController;

  public Action onRigProfileApplied;
  public Action onRigReset;

  void Start()
  {
    rigController = XRRigController.Instance;

    if (profile.inputActions != null)
      profile.inputActions.Disable();

    // flatScreenPCVersion = VersionSwitcher.Instance.isPCVersion;
  }

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;

    if (flatScreenPCVersion)
    {
      playerController = other.GetComponent<BasicFPCC>();
      if (pcSpawnPoint != null)
        playerController.transform.SetPositionAndRotation(pcSpawnPoint.position, pcSpawnPoint.rotation);

      if (disableMovement)
        playerController.EnableMovement(false);

      if (constraintSource != null)
      {
        PositionConstraint constraint = playerController.GetComponent<PositionConstraint>();
        constraint.AddSource(new ConstraintSource { sourceTransform = constraintSource, weight = 1 });
        constraint.constraintActive = true;
      }

      onRigProfileApplied?.Invoke();
      return;
    }

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

    if (flatScreenPCVersion && playerController != null)
    {
      if (pcResetPoint != null)
        playerController.transform.SetPositionAndRotation(pcResetPoint.position, pcResetPoint.rotation);
      playerController.EnableMovement(true);

      if (constraintSource != null)
      {
        PositionConstraint constraint = playerController.GetComponent<PositionConstraint>();
        constraint.constraintActive = false;
        if (constraint.sourceCount > 0)
          constraint.RemoveSource(0);
      }

      onRigReset?.Invoke();
      return;
    }

    if (rigController != null)
    {
      rigController.ResetRig(resetPoint);

      if (profile.inputActions != null)
        profile.inputActions.Disable();

      onRigReset?.Invoke();
    }
  }
}