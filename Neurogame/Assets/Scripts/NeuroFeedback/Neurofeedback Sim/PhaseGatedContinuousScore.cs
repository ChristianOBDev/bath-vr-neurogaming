using TMPro;
using UnityEngine;
using NeuroFeedback;
using System.Collections;

public class PhaseGatedContinuousScore : MonoBehaviour
{
    public static PhaseGatedContinuousScore Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private bool showMessageBeforeStart = true;
    [SerializeField] private string waitingMessage = "Get Ready...";

    [Header("Boss UI")]
    [SerializeField] private string bossFightMessage = "Boss Fight!";
    [SerializeField] private float bossMessageDuration = 2f;

    [Header("Phase Gate")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private PhaseManager.Phase startScoringPhase = PhaseManager.Phase.Phase2_Assisted;

    [Header("Bonuses")]
    [SerializeField] private int pointsPerHit = 20;
    [SerializeField] private int pointsPerKill = 120;

    [Tooltip("Bonus awarded when a shot is fired in Phase2+.")]
    [SerializeField] private int baseShotBonus = 80;

    [Header("Boss Scoring")]
    [SerializeField] private int shipsBeforeBoss = 3;
    [SerializeField] private int bossHitsToDestroy = 5;
    [SerializeField] private int pointsPerBossHit = 50;
    [SerializeField] private int pointsPerBossKill = 1000;

    [Header("Display Smoothing")]
    [SerializeField] private float displayLerpSpeed = 12f;

    [Header("Optional Sources (drag in)")]
    [SerializeField] private NeuroFeedbackController feedback;
    [SerializeField] private NeuroMeter meter;

    private float score;
    private float displayedScore;

    private int shipsDestroyedSinceBoss;
    private int currentBossHits;
    private bool bossActive;
    private bool showingTemporaryMessage;
    private Coroutine messageRoutine;

    public bool ScoringActive =>
        phaseManager != null && phaseManager.CurrentPhase >= startScoringPhase;

    public bool BossActive => bossActive;

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
            {
                scoreText.text = waitingMessage;
            }
            else if (!showingTemporaryMessage)
            {
                if (bossActive)
                    scoreText.text = $"Boss HP: {Mathf.Max(0, bossHitsToDestroy - currentBossHits)}  |  Score: {Mathf.FloorToInt(displayedScore)}";
                else
                    scoreText.text = Mathf.FloorToInt(displayedScore).ToString();
            }
        }
    }

    public void AddHit()
    {
        if (!ScoringActive) return;

        if (bossActive)
        {
            AddBossHit();
            return;
        }

        score += pointsPerHit;
    }

    public void AddKill()
    {
        if (!ScoringActive) return;

        if (bossActive)
        {
            // Ignore normal kill calls while boss is active.
            return;
        }

        score += pointsPerKill;

        shipsDestroyedSinceBoss++;

        if (shipsDestroyedSinceBoss >= shipsBeforeBoss)
        {
            StartBossFight();
        }
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

    private void StartBossFight()
    {
        bossActive = true;
        currentBossHits = 0;

        ShowTemporaryMessage(bossFightMessage, bossMessageDuration);

        // Put your boss spawn logic here, for example:
        // BossSpawner.Instance.SpawnBoss();
        // NormalShipSpawner.Instance.StopSpawning();
    }

    public void AddBossHit()
    {
        if (!ScoringActive || !bossActive) return;

        currentBossHits++;
        score += pointsPerBossHit;

        if (currentBossHits >= bossHitsToDestroy)
        {
            DestroyBoss();
        }
    }

    private void DestroyBoss()
    {
        bossActive = false;
        currentBossHits = 0;
        shipsDestroyedSinceBoss = 0;

        score += pointsPerBossKill;

        // Put your boss despawn / normal wave reset logic here, for example:
        // BossSpawner.Instance.DespawnBoss();
        // NormalShipSpawner.Instance.StartSpawning();
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowTemporaryMessageRoutine(message, duration));
    }

    private IEnumerator ShowTemporaryMessageRoutine(string message, float duration)
    {
        showingTemporaryMessage = true;

        if (scoreText != null)
            scoreText.text = message;

        yield return new WaitForSeconds(duration);

        showingTemporaryMessage = false;
        messageRoutine = null;
    }

    public void ResetScore()
    {
        score = 0f;
        displayedScore = 0f;

        shipsDestroyedSinceBoss = 0;
        currentBossHits = 0;
        bossActive = false;
        showingTemporaryMessage = false;

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }
    }
}