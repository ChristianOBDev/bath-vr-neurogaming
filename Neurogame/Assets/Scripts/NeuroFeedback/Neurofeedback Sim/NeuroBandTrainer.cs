using UnityEngine;
using NeuroFeedback;

public class NeuroBandTrainer : MonoBehaviour
{
    [Header("Signal Source (implements INeuroSignal)")]
    [SerializeField] private MonoBehaviour signalBehaviour;
    private INeuroSignal signal;

    [Header("Readout")]
    [Range(0f, 1f)] public float brain01;

    [Header("Band (Stationary per trial)")]
    [Range(0.08f, 0.65f)] public float bandWidth = 0.22f;
    [Range(0f, 1f)] public float bandCenter01 = 0.5f;
    public float bandMin01 { get; private set; }
    public float bandMax01 { get; private set; }

    [Header("Soft Edge")]
    [Range(0f, 0.25f)] public float softEdge = 0.06f;

    [Header("Charge")]
    [Range(0f, 1f)] public float charge01;
    public float fillPerSecond = 0.65f;
    public float drainPerSecond = 0.20f;
    public float chargeSmooth = 10f;

    [Header("Outputs")]
    [Range(0f, 1f)] public float inBand01;

    [Header("Adaptive Difficulty (easy-first)")]
    [Tooltip("0..1. Higher makes it easier (wider band, closer to player's current focus).")]
    [Range(0f, 1f)] public float easeBias = 0.85f;

    [Tooltip("Min/max band width bounds for safety.")]
    public Vector2 bandWidthRange = new Vector2(0.18f, 0.38f);

    [Tooltip("How much band center is allowed to move away from player's current focus at trial start.")]
    [Range(0f, 0.5f)] public float maxCenterOffset = 0.15f;

    [Tooltip("If focus improves a lot, we gently tighten band over trials (not within trial).")]
    [Range(0f, 0.15f)] public float tightenPerGoodTrial = 0.03f;

    [Tooltip("If focus is struggling, widen band next trial.")]
    [Range(0f, 0.15f)] public float widenPerBadTrial = 0.05f;

    // Track recent performance so next trial can be easier/harder
    private float lastTrialScore01 = 0.5f;

    void Awake()
    {
        signal = signalBehaviour as INeuroSignal;
        if (signal == null) Debug.LogError("NeuroBandTrainer: signalBehaviour must implement INeuroSignal.");

        RecomputeBand();
    }

    void Update()
    {
        if (signal == null) return;

        brain01 = Mathf.Clamp01(signal.Alpha);

        // Band is stationary; just recompute min/max from current center/width
        RecomputeBand();

        inBand01 = ComputeSoftBandScore(brain01, bandMin01, bandMax01, softEdge);

        float drive = (inBand01 > 0.001f) ? (inBand01 * fillPerSecond) : (-drainPerSecond);
        float targetCharge = Mathf.Clamp01(charge01 + drive * Time.deltaTime);

        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, chargeSmooth) * Time.deltaTime);
        charge01 = Mathf.Lerp(charge01, targetCharge, k);
    }

    private void RecomputeBand()
    {
        float half = bandWidth * 0.5f;
        bandMin01 = Mathf.Clamp01(bandCenter01 - half);
        bandMax01 = Mathf.Clamp01(bandCenter01 + half);
    }

    private float ComputeSoftBandScore(float x, float min, float max, float edge)
    {
        if (x >= min && x <= max) return 1f;
        if (edge <= 0.0001f) return 0f;

        if (x < min) return Mathf.InverseLerp(min - edge, min, x);
        return Mathf.InverseLerp(max + edge, max, x);
    }

    /// <summary>
    /// Call this at the start of each trial to choose a new, friendly stationary band.
    /// </summary>
    public void SetNewBandForTrial(PhaseManager.Phase phase, float currentFocus01)
    {
        // Phase settings: Phase1 easiest, Phase3 still friendly but a bit tighter.
        float phaseEase = phase switch
        {
            PhaseManager.Phase.Phase1_Baseline => 1.00f,
            PhaseManager.Phase.Phase2_Assisted => 0.90f,
            _ => 0.80f, // Phase3_Full
        };

        // Performance-based ease adjustment
        // If last trial was low, increase ease a bit; if high, slightly reduce ease.
        float perfEase = Mathf.Lerp(1.05f, 0.95f, lastTrialScore01);

        float ease = Mathf.Clamp01(easeBias * phaseEase * perfEase);

        // Width: wider when easier
        float wMin = bandWidthRange.x;
        float wMax = bandWidthRange.y;
        bandWidth = Mathf.Lerp(wMin, wMax, ease);

        // Center: random, but biased near player's current focus so it feels achievable
        float offset = Random.Range(-maxCenterOffset, maxCenterOffset);

        // Bias offset toward 0 (closer to focus) when ease is high
        offset *= Mathf.Lerp(1f, 0.35f, ease);

        float center = Mathf.Clamp01(currentFocus01 + offset);

        // Also nudge center slightly upward as player improves (but gentle)
        center = Mathf.Clamp01(center + Mathf.Lerp(-0.03f, 0.06f, lastTrialScore01));

        bandCenter01 = center;

        RecomputeBand();
    }

    /// <summary>
    /// Call at end of trial to update difficulty for next band pick.
    /// trialScore01 should be 0..1 representing how well player maintained focus.
    /// </summary>
    public void ReportTrialResult(float trialScore01)
    {
        lastTrialScore01 = Mathf.Clamp01(trialScore01);

        // Adapt width slightly for next trials (never too harsh)
        if (lastTrialScore01 >= 0.70f)
            bandWidth = Mathf.Clamp(bandWidth - tightenPerGoodTrial, bandWidthRange.x, bandWidthRange.y);
        else if (lastTrialScore01 <= 0.35f)
            bandWidth = Mathf.Clamp(bandWidth + widenPerBadTrial, bandWidthRange.x, bandWidthRange.y);

        RecomputeBand();
    }
}