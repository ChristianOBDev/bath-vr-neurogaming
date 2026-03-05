using System.Collections.Generic;
using UnityEngine;

public class NeuroFeedbackController : MonoBehaviour
{
    [Header("Signal")]
    [SerializeField] private MonoBehaviour signalBehaviour; // must implement INeuroSignal
    private INeuroSignal signal;

    [Header("Calibration Source")]
    [SerializeField] private NeuroCalibrator calibrator;

    [Header("Target Zone (alphaNorm 0..1)")]
    [Range(0f, 1f)] public float targetMin = 0.45f;
    [Range(0f, 1f)] public float targetMax = 0.65f;

    [Header("Zone Smoothing")]
    [Tooltip("Adds a soft edge so being slightly outside the zone doesn't become instantly 0.")]
    [Range(0f, 0.25f)] public float softEdge = 0.06f;

    [Header("Hysteresis")]
    [Tooltip("Extra margin for staying 'in zone' once entered (prevents flicker).")]
    [Range(0f, 0.25f)] public float hysteresis = 0.06f;

    [Header("Stability Window")]
    [Tooltip("Seconds of recent alphaNorm used to measure variance.")]
    public float varianceWindowSeconds = 1.5f;

    [Tooltip("How long player must stay stable to be considered 'locked in'.")]
    public float requiredStableSeconds = 1.2f;

    [Header("Quality Gate")]
    [Range(0f, 1f)] public float minQuality = 0.65f;

    [Header("Variance Tune")]
    [Tooltip("Variance at/above this is considered bad (score near 0).")]
    public float badVariance = 0.02f;

    [Tooltip("Variance at/below this is considered good (score near 1).")]
    public float goodVariance = 0.003f;

    [Header("Output Smoothing")]
    [Tooltip("How quickly stabilityScore follows the raw value.")]
    public float stabilitySmooth = 8f;

    [Header("Outputs (read-only at runtime)")]
    [Range(0f, 1f)] public float alphaNorm;
    [Range(0f, 1f)] public float stabilityScore; // 0..1
    public float stableTime;

    [Header("Debug")]
    public bool debugLogs = true;

    private readonly Queue<(float time, float value)> window = new();
    private float dbgT;

    private bool wasInZone = false; // hysteresis memory (per instance)

    public bool IsLockedStable => stableTime >= requiredStableSeconds;

    void Awake()
    {
        signal = signalBehaviour as INeuroSignal;
        if (signal == null)
            Debug.LogError("NeuroFeedbackController: signalBehaviour must implement INeuroSignal.");
    }

    void Update()
    {
        if (signal == null || calibrator == null) return;

        // ---- 1) Normalize alpha using calibration ----
        float z = (signal.Alpha - calibrator.alphaMean) / Mathf.Max(0.0001f, calibrator.alphaStd);

        // Map z to 0..1-ish. (0.2 = sensitivity; raise to make more responsive)
        alphaNorm = Mathf.Clamp01(0.5f + 0.2f * z);

        // ---- 2) Maintain window for variance ----
        window.Enqueue((Time.time, alphaNorm));
        while (window.Count > 0 && Time.time - window.Peek().time > varianceWindowSeconds)
            window.Dequeue();

        float var = ComputeVariance(window);

        // ---- 3) Zone check with hysteresis ----
        float enterMin = targetMin;
        float enterMax = targetMax;
        float stayMin = Mathf.Clamp01(targetMin - hysteresis);
        float stayMax = Mathf.Clamp01(targetMax + hysteresis);

        bool enterZone = (alphaNorm >= enterMin && alphaNorm <= enterMax);
        bool stayZone = (alphaNorm >= stayMin && alphaNorm <= stayMax);

        bool inZone = wasInZone ? stayZone : enterZone;
        wasInZone = inZone;

        // ---- 4) Soft zone scoring (so slightly outside isn't instantly 0) ----
        // Score = 1 inside [targetMin,targetMax], then falls off within +/- softEdge
        float zoneScore = ComputeSoftZoneScore(alphaNorm, targetMin, targetMax, softEdge);

        // ---- 5) Quality score ----
        float qScore = 0f;
        if (signal.Quality >= minQuality)
            qScore = Mathf.Clamp01((signal.Quality - minQuality) / (1f - minQuality));

        // ---- 6) Variance score (lower variance => higher score) ----
        float varScore = Mathf.InverseLerp(badVariance, goodVariance, var);
        varScore = Mathf.Clamp01(varScore);

        // ---- 7) Combine into raw stability ----
        // If hysteresis says you're "in zone", you get full zoneScore. If not, zoneScore still gives partial credit.
        float zoneGate = inZone ? 1f : zoneScore;

        float raw = zoneGate * varScore * qScore;

        // Smooth stability so it doesn't chatter
        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, stabilitySmooth) * Time.deltaTime);
        stabilityScore = Mathf.Lerp(stabilityScore, raw, k);

        // ---- 8) Stable-time latch ----
        if (stabilityScore > 0.6f)
            stableTime += Time.deltaTime;
        else
            stableTime = Mathf.Max(0f, stableTime - 2f * Time.deltaTime);

        // ---- Debug ----
        if (debugLogs)
        {
            dbgT += Time.deltaTime;
            if (dbgT >= 1f)
            {
                dbgT = 0f;
                Debug.Log(
                    $"[NEURO] alphaNorm:{alphaNorm:F2} " +
                    $"inZone:{inZone} (enter:{enterMin:F2}-{enterMax:F2}, stay:{stayMin:F2}-{stayMax:F2}) " +
                    $"zoneScore:{zoneScore:F2} var:{var:F4} varScore:{varScore:F2} " +
                    $"Q:{signal.Quality:F2} qScore:{qScore:F2} " +
                    $"stab:{stabilityScore:F2} stableTime:{stableTime:F2} locked:{IsLockedStable}"
                );
            }
        }
    }

    private float ComputeVariance(Queue<(float time, float value)> q)
    {
        if (q.Count < 5) return 1f;

        float mean = 0f;
        foreach (var e in q) mean += e.value;
        mean /= q.Count;

        float v = 0f;
        foreach (var e in q)
        {
            float d = e.value - mean;
            v += d * d;
        }
        return v / Mathf.Max(1, q.Count - 1);
    }

    private float ComputeSoftZoneScore(float x, float min, float max, float edge)
    {
        if (edge <= 0.0001f)
        {
            // hard gate
            return (x >= min && x <= max) ? 1f : 0f;
        }

        // inside zone
        if (x >= min && x <= max) return 1f;

        // below min: fade from 1 at min to 0 at min-edge
        if (x < min)
            return Mathf.InverseLerp(min - edge, min, x);

        // above max: fade from 1 at max to 0 at max+edge
        return Mathf.InverseLerp(max + edge, max, x);
    }
}
