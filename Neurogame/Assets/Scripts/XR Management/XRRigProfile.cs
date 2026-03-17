using UnityEngine;
using UnityEngine.InputSystem;

public enum ControllerVisuals
{
  Default,
  MotorImagery,
  MVEP,
  NeuroFeedback
}

[CreateAssetMenu(menuName = "ScriptableObjects/Rig Profile")]
public class XRRigProfile : ScriptableObject
{

  [Header("Camera")]
  public Vector3 cameraOffset = Vector3.zero;

  [Header("Locomotion")]
  public bool enableContinuousMove = false;
  public bool enableTeleport = false;
  public bool enableSnapTurn = false;

  [Header("Interactors")]
  public bool enablePokeInteractor = false;
  public bool enableNearFarInteractor = false;

  [Header("Visuals")]
  public ControllerVisuals controllerVisuals = ControllerVisuals.Default;

  [Header("Input")]
  public InputActionAsset inputActions;
}
