using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "XR/Rig Profile")]
public class XRRigProfile : ScriptableObject
{

  [Header("Camera")]
  public Vector3 cameraOffset = Vector3.zero;

  [Header("Locomotion")]
  public bool enableContinuousMove = false;
  public bool enableTeleport = false;
  public bool enableSnapTurn = false;

  [Header("Input")]
  public InputActionAsset inputActions;
}
