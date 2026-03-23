using UnityEngine;

/// <summary>
/// Bridges UDPManager EEG data to KickerInputRouter.
/// Replaces or blends with thumbstick input depending on mode.
/// Data format confirmed: Single float, bipolar (-1 = left, +1 = right)
/// </summary>
public class EEGKickerInput : MonoBehaviour
{
  [Header("References")]
  public KickerInputRouter inputRouter;

  [Header("EEG Settings")]
  [Tooltip("Enable to use EEG input instead of thumbstick")]
  public bool eegEnabled = false;

  [Tooltip("Normalise incoming values to 0-1 range")]
  public bool normaliseInput = true;

  [Tooltip("Expected maximum raw signal value (used for normalisation)")]
  public float rawSignalMax = 1f;

  [Tooltip("Minimum signal threshold before registering as input")]
  [Range(0f, 1f)]
  public float signalDeadzone = 0.1f;

  [Tooltip("Blend between EEG (1) and thumbstick (0) input")]
  [Range(0f, 1f)]
  public float eegBlendWeight = 1f;

  [Header("Debug")]
  public bool eegDebugLogging = false;

  // Current normalised EEG values
  private float leftSignal = 0f;
  private float rightSignal = 0f;

  [Range(-1f, 1f)]
  public float testInputValue = 0f; // For testing in editor

  void OnEnable()
  {
    if (UDPManager.Instance == null)
    {
      Debug.LogWarning("EEGKickerInput: UDPManager.Instance is null on OnEnable.");
      return;
    }
    UDPManager.Instance.OnFloatReceived += HandleFloat;
  }

  void OnDisable()
  {
    if (UDPManager.Instance == null) return;
    UDPManager.Instance.OnFloatReceived -= HandleFloat;
  }

  void HandleFloat(float value)
  {
    testInputValue = value; // For editor testing

    float normalised = normaliseInput ? value / rawSignalMax : value;
    normalised = Mathf.Clamp(normalised, -1f, 1f);

    leftSignal = normalised < -signalDeadzone ? Mathf.Abs(normalised) : 0f;
    rightSignal = normalised > signalDeadzone ? normalised : 0f;

    ApplyToRouter(normalised);

    if (eegDebugLogging)
      Debug.Log($"EEG Float: raw={value}, normalised={normalised}, L={leftSignal}, R={rightSignal}");
  }

  void ApplyToRouter(float eegControlValue)
  {
    if (!eegEnabled || inputRouter == null) return;

    if (eegBlendWeight >= 1f)
    {
      // Full EEG control
      inputRouter.controlValue = eegControlValue;
    }
    else
    {
      // Blend EEG with current thumbstick value
      inputRouter.controlValue = Mathf.Lerp(
          inputRouter.controlValue,
          eegControlValue,
          eegBlendWeight
      );
    }
  }

  // Called externally to get current signal strength for a given side
  // Useful for glow effects or other visual feedback
  public float GetSignal(bool isLeft)
  {
    return isLeft ? leftSignal : rightSignal;
  }
}