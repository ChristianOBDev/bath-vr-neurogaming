using UnityEngine;

public class NeuroInputSignal : MonoBehaviour
{
    public KeyCode inputKey = KeyCode.Space;

    [Header("Signal")]
    [Range(0f, 1f)] public float signal;
    public float increasePerPress = 0.12f;
    public float decayPerSecond = 0.4f;

    private void Start()
    {
        // Ensures neuro bar starts from bottom
        signal = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(inputKey))
            signal += increasePerPress;

        signal -= decayPerSecond * Time.deltaTime;
        signal = Mathf.Clamp01(signal);
    }

    public float GetSignal()
    {
        return signal;
    }
}