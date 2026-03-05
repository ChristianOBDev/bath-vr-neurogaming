using UnityEngine;

public class NeuroMeter : MonoBehaviour
{
    [SerializeField] private NeuroFeedbackController neuro;

    [Header("Meter Value")]
    [Range(0f, 1f)] public float meterValue;

    [Header("Rates")]
    [Tooltip("Max fill per second when stabilityScore is 1.")]
    public float maxFillRate = 0.35f;

    [Tooltip("Max drain per second when stabilityScore is 0.")]
    public float maxDrainRate = 0.18f;

    [Header("Control Curve")]
    [Tooltip("Stability below this drains more than fills (prevents tiny noise from filling).")]
    [Range(0f, 1f)] public float neutralPoint = 0.55f;

    [Tooltip("How wide the deadzone is around neutralPoint (no change).")]
    [Range(0f, 0.3f)] public float deadZone = 0.05f;

    [Header("Smoothing")]
    [Tooltip("Higher = meter changes feel heavier/slower.")]
    public float meterSmooth = 10f;

    [Header("Optimal Band (Green Zone)")]
    [Range(0f, 1f)] public float optimalMin = 0.55f;
    [Range(0f, 1f)] public float optimalMax = 0.75f;

    [Tooltip("Optional: require holding inside optimal band for this many seconds.")]
    public float requiredOptimalHold = 0.6f;

    public float optimalHoldTime { get; private set; }
    public bool InOptimalBand => meterValue >= optimalMin && meterValue <= optimalMax;
    public bool OptimalReady => optimalHoldTime >= requiredOptimalHold;

    [Header("Debug")]
    public bool debugLogs = true;

    float dbgT;

    void Update()
    {
        if (neuro == null) return;

        float s = Mathf.Clamp01(neuro.stabilityScore); // 0..1 continuous

        // Convert stability into a signed drive value (-1..+1)
        float drive = (s - neutralPoint) / Mathf.Max(0.0001f, (1f - neutralPoint));
        drive = Mathf.Clamp(drive, -1f, 1f);

        // Deadzone around neutral (prevents jitter)
        if (Mathf.Abs(s - neutralPoint) < deadZone)
            drive = 0f;

        // Target delta per second
        float rate = (drive >= 0f)
            ? (drive * maxFillRate)
            : (drive * maxDrainRate); // drive is negative => drains

        float target = Mathf.Clamp01(meterValue + rate * Time.deltaTime);

        // Smooth towards target so it doesn’t snap
        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, meterSmooth) * Time.deltaTime);
        meterValue = Mathf.Lerp(meterValue, target, k);

        // Track time spent inside the optimal band
        if (InOptimalBand) optimalHoldTime += Time.deltaTime;
        else optimalHoldTime = 0f;

        if (debugLogs)
        {
            dbgT += Time.deltaTime;
            if (dbgT >= 1f)
            {
                dbgT = 0f;
                Debug.Log($"[METER] meter:{meterValue:F2} | stability:{s:F2} | drive:{drive:F2} | rate/s:{rate:F2} | optimal:{(InOptimalBand ? "YES" : "no")} hold:{optimalHoldTime:F2}");
            }
        }
    }
}