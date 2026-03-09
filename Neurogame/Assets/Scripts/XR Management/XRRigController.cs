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

  XRRigProfile activeProfile;

  public void ApplyProfileSettings(XRRigProfile profile)
  {
    activeProfile = profile;

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


}
