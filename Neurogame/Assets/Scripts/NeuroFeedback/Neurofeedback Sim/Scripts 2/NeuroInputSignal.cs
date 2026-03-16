using UnityEngine;
using UnityEngine.InputSystem;

public class NeuroInputSignal : MonoBehaviour
{
    public KeyCode inputKey = KeyCode.Space;

    [Header("XRI Input")]
    public InputActionAsset xriInputActions;
    public string actionMapName = "XRI RightHand";
    public string actionName = "Activate";

    [Header("Signal")]
    [Range(0f, 1f)] public float signal;
    public float increasePerPress = 0.12f;
    public float decayPerSecond = 0.4f;

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
    }

    private void OnDisable()
    {
        xrAction?.Disable();
    }

    private void Start()
    {
        signal = 0f;
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

        signal -= decayPerSecond * Time.deltaTime;
        signal = Mathf.Clamp01(signal);
    }

    public float GetSignal()
    {
        return signal;
    }
}