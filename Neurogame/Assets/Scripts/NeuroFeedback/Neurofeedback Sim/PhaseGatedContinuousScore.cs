using TMPro;
using UnityEngine;
using NeuroFeedback;

public class PhaseGatedContinuousScore : MonoBehaviour
{
    public static PhaseGatedContinuousScore Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private bool showMessageBeforeStart = true;
    [SerializeField] private string waitingMessage = "Get Ready...";

    [Header("Phase Gate")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private PhaseManager.Phase startScoringPhase = PhaseManager.Phase.Phase2_Assisted;

    [Header("Bonuses")]
    [SerializeField] private int pointsPerHit = 20;
    [SerializeField] private int pointsPerKill = 120;

    [Tooltip("Bonus awarded when a shot is fired in Phase2+.")]
    [SerializeField] private int baseShotBonus = 80;

    [Header("Display Smoothing")]
    [SerializeField] private float displayLerpSpeed = 12f;

    [Header("Optional Sources (drag in)")]
    [SerializeField] private NeuroFeedbackController feedback;
    [SerializeField] private NeuroMeter meter;

    private float score;
    private float displayedScore;

    public bool ScoringActive =>
        phaseManager != null && phaseManager.CurrentPhase >= startScoringPhase;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (phaseManager == null) phaseManager = FindFirstObjectByType<PhaseManager>();
        if (feedback == null) feedback = FindFirstObjectByType<NeuroFeedbackController>();
        if (meter == null) meter = FindFirstObjectByType<NeuroMeter>();

        if (scoreText != null && showMessageBeforeStart)
            scoreText.text = waitingMessage;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        float k = 1f - Mathf.Exp(-displayLerpSpeed * Time.deltaTime);
        displayedScore = Mathf.Lerp(displayedScore, score, k);

        if (scoreText != null)
        {
            if (!ScoringActive && showMessageBeforeStart)
                scoreText.text = waitingMessage;
            else
                scoreText.text = Mathf.FloorToInt(displayedScore).ToString();
        }
    }

    public void AddHit()
    {
        if (!ScoringActive) return;
        score += pointsPerHit;
    }

    public void AddKill()
    {
        if (!ScoringActive) return;
        score += pointsPerKill;
    }

    public void OnShotFired(float stability01, bool optimal, float hitChance01)
    {
        if (!ScoringActive) return;

        float s = Mathf.Clamp01(stability01);
        float c = Mathf.Clamp01(hitChance01);

        float mult = 1f;
        mult += 0.75f * s;
        mult += 0.50f * c;
        if (optimal) mult += 0.40f;

        score += baseShotBonus * mult;
    }

    public void ResetScore()
    {
        score = 0f;
        displayedScore = 0f;
    }
}