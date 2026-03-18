using UnityEngine;
using NeuroCONCISE;

/// <summary>
/// Bridges UDPManager EEG data to KickerInputRouter.
/// Replaces or blends with thumbstick input depending on mode.
/// Awaiting confirmation of EEG data format before full implementation.
/// </summary>
public class EEGKickerInput : MonoBehaviour
{
    [Header("References")]
    public KickerInputRouter inputRouter;

    [Header("EEG Settings")]
    [Tooltip("Enable to use EEG input instead of thumbstick")]
    public bool eegEnabled = false;

    [Tooltip("Index of left motor imagery value in float array (if applicable)")]
    public int leftChannelIndex = 0;

    [Tooltip("Index of right motor imagery value in float array (if applicable)")]
    public int rightChannelIndex = 1;

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

    // TODO: Confirm data format with EEG middleware team
    // Expected format options:
    // A) Single float: bipolar value (-1 = left, +1 = right)
    // B) Float array: [leftValue, rightValue, ...otherChannels]
    // C) Int: discrete class label (e.g. 1 = left, 2 = right, 0 = rest)

    void OnEnable()
    {
        if (UDPManager.Instance == null) return;
        UDPManager.Instance.OnFloatReceived += HandleFloat;
    }

    void OnDisable()
    {
        if (UDPManager.Instance == null) return;
        UDPManager.Instance.OnFloatReceived -= HandleFloat;
    }

    // ---- Option A: Single float handler ----
    // Use if EEG sends a single bipolar float (-1 to 1)
    // Negative = left motor imagery, Positive = right motor imagery
    void HandleFloat(float value)
    {
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