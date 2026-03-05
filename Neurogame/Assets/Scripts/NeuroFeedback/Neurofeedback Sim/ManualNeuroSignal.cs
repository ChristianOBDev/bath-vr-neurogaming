using UnityEngine;

public class ManualNeuroSignal : MonoBehaviour, INeuroSignal
{
    [Header("Signal Level (0..1)")]
    [Range(0f, 1f)] public float level01 = 0.15f;

    [Header("Tap Control")]
    [Tooltip("Tap this key repeatedly to increase the signal.")]
    public KeyCode pumpKey = KeyCode.Space;

    [Tooltip("Optional: tap this to decrease faster.")]
    public KeyCode dumpKey = KeyCode.LeftShift;

    [Header("Tuning")]
    [Tooltip("How much each tap increases the signal (0..1).")]
    public float pumpAmountPerTap = 0.06f;

    [Tooltip("How fast signal drains per second when you stop tapping.")]
    public float drainPerSecond = 0.22f;

    [Tooltip("Extra drain per tap when pressing dumpKey (makes it drop quicker).")]
    public float dumpAmountPerTap = 0.10f;

    [Header("Quality")]
    [Range(0f, 1f)] public float quality = 1f;

    // INeuroSignal: we use Alpha as our "brain activity" level for the signal bar
    public float Alpha => level01;
    public float Beta => 0.5f;
    public float Theta => 0.5f;
    public float Quality => quality;

    void Update()
    {
        // Drain continuously if not maintained
        level01 -= drainPerSecond * Time.deltaTime;

        // Pump up by tapping
        if (Input.GetKeyDown(pumpKey))
            level01 += pumpAmountPerTap;

        // Optional: quick dump down (tap)
        if (Input.GetKeyDown(dumpKey))
            level01 -= dumpAmountPerTap;

        level01 = Mathf.Clamp01(level01);
    }
}