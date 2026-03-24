using UnityEngine;
using UnityEngine.InputSystem;

public class NeuroInputSignal : MonoBehaviour, ISignalProvider, IResettableSignal
{
    [Header("Keyboard Input")]
    public Key inputKey = Key.Space;

    [Header("XRI Input")]
    public InputActionAsset xriInputActions;
    public string actionMapName = "XRI RightHand";
    public string actionName = "Activate";

    [Header("UDP Input")]
    public bool useUdp = true;

    [Tooltip("Incoming UDP minimum raw value.")]
    public float udpMin = -1f;

    [Tooltip("Incoming UDP maximum raw value.")]
    public float udpMax = 1f;

    [Tooltip("Invert after mapping if needed.")]
    public bool invertUdp = false;

    [Header("Signal")]
    [Range(0f, 1f)] public float signal = 0f;
    public float increasePerPress = 0.12f;
    public float udpRiseSpeed = 1.2f;
    public float decayPerSecond = 0.25f;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    [Range(0f, 1f)] public float udpSignal = 0f;
    public float lastReceivedUdpValue = 0f;

    private InputAction xrAction;
    private float debugLogTimer;

    private void Awake()
    {
        if (xriInputActions != null)
            xrAction = xriInputActions.FindActionMap(actionMapName)?.FindAction(actionName);
    }

    private void OnEnable()
    {
        xrAction?.Enable();

        if (useUdp && UDPManager.Instance != null)
        {
            UDPManager.Instance.OnFloatReceived -= HandleUdpFloat;
            UDPManager.Instance.OnFloatReceived += HandleUdpFloat;

            if (enableDebugLogs)
                Debug.Log("[NeuroInputSignal] Subscribed to UDPManager.OnFloatReceived");
        }

        ResetSignal();
    }

    private void OnDisable()
    {
        xrAction?.Disable();

        if (UDPManager.Instance != null)
        {
            UDPManager.Instance.OnFloatReceived -= HandleUdpFloat;

            if (enableDebugLogs)
                Debug.Log("[NeuroInputSignal] Unsubscribed from UDPManager");
        }
    }

    private void Update()
    {
        bool pressed = false;

        if (Keyboard.current != null && Keyboard.current[inputKey].wasPressedThisFrame)
            pressed = true;

        if (xrAction != null && xrAction.WasPressedThisFrame())
            pressed = true;

        if (pressed)
        {
            signal += increasePerPress;

            if (enableDebugLogs)
                Debug.Log($"[NeuroInputSignal] Input press -> +{increasePerPress:F3}");
        }

        if (useUdp)
            signal += udpSignal * udpRiseSpeed * Time.deltaTime;

        signal -= decayPerSecond * Time.deltaTime;
        signal = Mathf.Clamp01(signal);

        if (enableDebugLogs)
        {
            debugLogTimer += Time.deltaTime;

            if (debugLogTimer >= 0.5f)
            {
                debugLogTimer = 0f;
                Debug.Log($"[NeuroInputSignal] FINAL SIGNAL: {signal:F3} | UDP: {udpSignal:F3} | Raw UDP: {lastReceivedUdpValue:F3}");
            }
        }
    }

    private void HandleUdpFloat(float value)
    {
        lastReceivedUdpValue = value;

        float mapped = Mathf.InverseLerp(udpMin, udpMax, value);

        if (invertUdp)
            mapped = 1f - mapped;

        udpSignal = Mathf.Clamp01(mapped);

        if (enableDebugLogs)
            Debug.Log($"[NeuroInputSignal] UDP RECEIVED -> raw: {value:F3} | mapped: {udpSignal:F3} | range: [{udpMin:F3}, {udpMax:F3}]");
    }

    public float GetSignal01()
    {
        return signal;
    }

    public void ResetSignal()
    {
        signal = 0f;
        udpSignal = 0f;
        lastReceivedUdpValue = 0f;

        if (enableDebugLogs)
            Debug.Log("[NeuroInputSignal] Signal Reset");
    }
}