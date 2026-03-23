using UnityEngine;
using UnityEngine.InputSystem;

public class KickerInputRouter : MonoBehaviour
{
  [Header("Debug / Auto-Fire")]
  [Tooltip("If true, the kickers always fire at full strength")]
  public bool autoFire = false;

  [Header("XR Input")]
  [Tooltip("Vector2 action bound to RightHand primary2DAxis")]
  public InputActionProperty kickerAxis;

  [Header("Input Processing")]
  [Range(0f, 0.5f)]
  public float deadZone = 0.1f;

  [Header("Debug")]
  [Range(-1f, 1f)]
  public float controlValue;

  void OnEnable()
  {
    if (kickerAxis.action != null)
      kickerAxis.action.Enable();
  }

  void OnDisable()
  {
    if (kickerAxis.action != null)
      kickerAxis.action.Disable();
  }

  // void Update()
  // {
  //     if (kickerAxis.action == null)
  //         return;

  //     Vector2 stick = kickerAxis.action.ReadValue<Vector2>();

  //     float raw = stick.x;

  //     if (Mathf.Abs(raw) < deadZone)
  //         raw = 0f;

  //     controlValue = Mathf.Clamp(raw, -1f, 1f);
  // }

  /// <summary>
  /// Returns force strength [0�1] for left or right kicker.
  /// </summary>
  public float GetStrength(bool isLeft, bool graduated)
  {
    // If autoFire is on, always return max strength for the appropriate kicker
    if (autoFire)
      return 1f;

    // Otherwise, use player input
    if (isLeft && controlValue >= 0f) return 0f;
    if (!isLeft && controlValue <= 0f) return 0f;

    float magnitude = Mathf.Abs(controlValue);
    return graduated ? magnitude : 1f;
  }
}
