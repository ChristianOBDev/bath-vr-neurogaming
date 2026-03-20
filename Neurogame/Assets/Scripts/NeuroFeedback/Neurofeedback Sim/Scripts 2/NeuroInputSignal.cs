using UnityEngine;
using UnityEngine.InputSystem;

public class NeuroInputSignal : MonoBehaviour, ISignalProvider, IResettableSignal
{
    [Header("Keyboard Input")]
    public KeyCode inputKey = KeyCode.Space;

    [Header("XRI Input")]
    public InputActionAsset xriInputActions;
    public string actionMapName = "XRI RightHand";
    public string actionName = "Activate";

    [Header("Signal")]
    [Range(0f, 1f)] public float signal;

    public float increasePerPress = 0.12f;
    public float decayPerSecond = 0.4f;

    [Header("UDP Input")]
    public bool useUdp = true;

    [Tooltip("Map incoming UDP values to 0-1")]
    public float udpMin = 0f;
    public float udpMax = 1f;

    private float udpValue = 0f;
    private InputAction xrAction;

    private void Awake()
    {
        if (xriInputActions != null)
        {
            xrAction = xriInputActions.FindActionMap(actionMapName)?.FindAction(actionName);
        }
    }

    private void OnEnable()
    {
        xrAction?.Enable();

        if (useUdp && UDPManager.Instance != null)
        {
            UDPManager.Instance.OnFloatReceived += HandleUdpFloat;
        }

        ResetSignal();
    }

    private void OnDisable()
    {
        xrAction?.Disable();

        if (UDPManager.Instance != null)
        {
            UDPManager.Instance.OnFloatReceived -= HandleUdpFloat;
        }
    }

    void Update()
    {
        bool pressed = false;

        if (Input.GetKeyDown(inputKey))
            pressed = true;

        if (xrAction != null && xrAction.WasPressedThisFrame())
            pressed = true;

        if (pressed)
            signal += increasePerPress;

        // decay
        signal -= decayPerSecond * Time.deltaTime;

        // add UDP influence
        signal += udpValue * Time.deltaTime;

        signal = Mathf.Clamp01(signal);
    }

    private void HandleUdpFloat(float value)
    {
        float normalized = Mathf.InverseLerp(udpMin, udpMax, value);
        udpValue = Mathf.Clamp01(normalized);
    }

    public float GetSignal01()
    {
        return signal;
    }

    public void ResetSignal()
    {
        signal = 0f;
        udpValue = 0f;
    }
}