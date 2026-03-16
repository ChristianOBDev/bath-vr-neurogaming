using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.Animations;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(XROrigin))]
[RequireComponent(typeof(PositionConstraint))]
public class XRRigController : Singleton<XRRigController>
{
  [Header("Rig")]
  public XROrigin xrOrigin;

  [Header("Locomotion")]
  public ContinuousMoveProvider moveProvider;
  public TeleportationProvider teleportProvider;
  public SnapTurnProvider snapTurnProvider;

  [Header("Interactors")]
  public XRPokeInteractor[] pokeInteractors;
  public NearFarInteractor[] nearFarInteractors;
  public XRRayInteractor[] teleportRayInteractors;

  [Header("Constraints")]
  public PositionConstraint positionConstraint;

  [Header("Visuals")]
  public GameObject[] defaultVisuals;
  public GameObject[] motorImageryVisuals;
  public GameObject[] mvepVisuals;
  public GameObject[] neuroFeedbackVisuals;

  [Header("Default Profile")]
  public XRRigProfile defaultProfile;

  private void Start()
  {
    if (xrOrigin == null)
      xrOrigin = GetComponent<XROrigin>();

    if (positionConstraint == null)
      positionConstraint = GetComponent<PositionConstraint>();


    if (defaultProfile != null)
    {
      ApplyProfileSettings(defaultProfile);
    }
    else
    {
      Debug.LogWarning("No default profile assigned to XRRigController. Please assign a default profile to ensure proper rig configuration.");
    }
  }

  public void ApplyProfileSettings(XRRigProfile profile)
  {
    ApplyCameraSettings(profile);
    ApplyLocomotion(profile);
    ApplyInteractors(profile);
    ApplyVisuals(profile);
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

  public void ApplyInteractors(XRRigProfile profile)
  {
    if (pokeInteractors != null)
      foreach (var interactor in pokeInteractors)
      {
        interactor.enabled = profile.enablePokeInteractor;
        interactor.gameObject.SetActive(profile.enablePokeInteractor);
      }

    if (nearFarInteractors != null)
      foreach (var interactor in nearFarInteractors)
      {
        interactor.enabled = profile.enableNearFarInteractor;
        interactor.gameObject.SetActive(profile.enableNearFarInteractor);
      }


    if (teleportRayInteractors != null)
      foreach (var interactor in teleportRayInteractors)
      {
        interactor.enabled = profile.enableTeleport;
        interactor.gameObject.SetActive(profile.enableTeleport);
      }
  }

  public void ApplyVisuals(XRRigProfile profile)
  {
    switch (profile.controllerVisuals)
    {
      case ControllerVisuals.Default:
        SetVisualsActive(defaultVisuals, true);
        SetVisualsActive(motorImageryVisuals, false);
        SetVisualsActive(mvepVisuals, false);
        SetVisualsActive(neuroFeedbackVisuals, false);
        break;
      case ControllerVisuals.MotorImagery:
        SetVisualsActive(defaultVisuals, false);
        SetVisualsActive(motorImageryVisuals, true);
        SetVisualsActive(mvepVisuals, false);
        SetVisualsActive(neuroFeedbackVisuals, false);
        break;
      case ControllerVisuals.MVEP:
        SetVisualsActive(defaultVisuals, false);
        SetVisualsActive(motorImageryVisuals, false);
        SetVisualsActive(mvepVisuals, true);
        SetVisualsActive(neuroFeedbackVisuals, false);
        break;
      case ControllerVisuals.NeuroFeedback:
        SetVisualsActive(defaultVisuals, false);
        SetVisualsActive(motorImageryVisuals, false);
        SetVisualsActive(mvepVisuals, false);
        SetVisualsActive(neuroFeedbackVisuals, true);
        break;
    }
  }

  private void SetVisualsActive(GameObject[] visuals, bool active)
  {
    if (visuals == null) return;

    foreach (var visual in visuals)
    {
      if (visual != null)
        visual.SetActive(active);
    }
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
      if (positionConstraint.sourceCount > 0) positionConstraint.RemoveSource(0);
      positionConstraint.enabled = false;
    }

    ApplyPositionAndRotation(newResetPoint);
    ApplyProfileSettings(defaultProfile);
  }
}
