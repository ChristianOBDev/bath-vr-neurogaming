using UnityEngine;
using UnityEngine.InputSystem;

public class KeySpamSignalProvider : MonoBehaviour, ISignalProvider, IResettableSignal
{
    public Key inputKey = Key.Space;

    [Header("Signal")]
    [Range(0f, 1f)] public float signal = 0f;
    public float increasePerPress = 0.12f;
    public float decayPerSecond = 0.4f;

    public void ResetSignal()
    {
        signal = 0f;
    }

    private void OnEnable()
    {
        ResetSignal();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[inputKey].wasPressedThisFrame)
            signal += increasePerPress;

        signal -= decayPerSecond * Time.deltaTime;
        signal = Mathf.Clamp01(signal);
    }

    public float GetSignal01() => signal;
}