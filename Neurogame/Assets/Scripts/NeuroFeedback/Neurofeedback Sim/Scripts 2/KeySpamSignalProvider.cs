using UnityEngine;

public class KeySpamSignalProvider : MonoBehaviour, ISignalProvider, IResettableSignal
{
    public KeyCode inputKey = KeyCode.Space;

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

    void Update()
    {
        if (Input.GetKeyDown(inputKey))
            signal += increasePerPress;

        signal -= decayPerSecond * Time.deltaTime;
        signal = Mathf.Clamp01(signal);
    }

    public float GetSignal01() => signal;
}