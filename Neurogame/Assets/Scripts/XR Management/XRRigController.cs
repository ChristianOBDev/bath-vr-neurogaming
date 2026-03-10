using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.Animations;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class XRRigController : Singleton<XRRigController>
{
  [Header("Rig")]
  public XROrigin xrOrigin;

  [Header("Locomotion")]
  public ContinuousMoveProvider moveProvider;
  public TeleportationProvider teleportProvider;
  public SnapTurnProvider snapTurnProvider;

  [Header("Constraints")]
  public PositionConstraint positionConstraint;

  [Header("Default Profile")]
  public XRRigProfile defaultProfile;

  public void ApplyProfileSettings(XRRigProfile profile)
  {
    ApplyCameraSettings(profile);
    ApplyLocomotion(profile);
    ApplyInput(profile);
  }

  public void ApplyCameraSettings(XRRigProfile profile)
  {
    xrOrigin.CameraYOffset = profile.cameraOffset.y;
  }

  public void ApplyLocomotion(XRRigProfile profile)
  {
    if (moveProvider != null)
      moveProvider.enabled = profile.enableContinuousMove;

    if (teleportProvider != null)
      teleportProvider.enabled = profile.enableTeleport;

    if (snapTurnProvider != null)
      snapTurnProvider.enabled = profile.enableSnapTurn;
  }

  public void ApplyInput(XRRigProfile profile)
  {
    if (profile.inputActions != null)
      profile.inputActions.Enable();
  }

  public void ApplyPositionAndRotation(Transform spawnPoint)
  {
    if (spawnPoint != null)
    {
      xrOrigin.MoveCameraToWorldLocation(spawnPoint.position);
      xrOrigin.MatchOriginUpCameraForward(spawnPoint.up, spawnPoint.forward);
    }
  }

  public void ApplyConstraints(Transform constraintSource, Vector3 constraintOffset)
  {
    if (positionConstraint != null)
    {
      positionConstraint.enabled = true;
      positionConstraint.AddSource(new ConstraintSource { sourceTransform = constraintSource, weight = 1 });
      positionConstraint.translationOffset = constraintOffset;
      positionConstraint.constraintActive = true;
    }
  }

  public void ResetRig(Transform newResetPoint)
  {
    Debug.Log("Resetting rig to default profile and position.");
    if (positionConstraint != null)
    {
      positionConstraint.constraintActive = false;
      positionConstraint.RemoveSource(0);
      positionConstraint.enabled = false;
    }

    ApplyPositionAndRotation(newResetPoint);
    ApplyProfileSettings(defaultProfile);
  }
}
