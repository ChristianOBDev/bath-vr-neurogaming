using UnityEngine;
using TMPro;
using NeuroFeedback;

public class NeuroChargeController : MonoBehaviour
{
    [Header("Signal Providers")]
    public SlowGimmickSignalProvider phase1Gimmick;
    public MonoBehaviour userSignalProviderBehaviour;

    [Header("UI Refs")]
    public NeuroMeterUI ui;
    public PillarDualCubeMeter pillarMeter3D;
    public NeuroCannonController cannon;
    public CannonBarrelFeedback barrelFeedback;

    [Header("3D Bar Refs")]
    public NeuroBar3D neuroBar3D;
    public bool syncWithUI = true;

    [Header("Phase Manager")]
    public PhaseManager phaseManager;

    [Header("Session")]
    public float sessionDuration = 30f;

    [Header("Timer (3D TextMeshPro)")]
    public TextMeshPro shotTimerText;

    [Header("Particle Timer Controller")]
    [Tooltip("Reference the GameObject that has the ParticleArrayDisintegrate script.")]
    public ParticleArrayDisintegrate particleDisintegrateTimer;

    [Header("Threshold Band")]
    [Range(0.02f, 0.5f)] public float bandSize = 0.18f;
    public Vector2 bandCenterRange = new Vector2(0.25f, 0.85f);

    [Header("Charge")]
    [Range(0f, 1f)] public float charge;
    public float chargeRate = 0.25f;

    [Header("Phase 1 Guidance")]
    [Range(0.1f, 1f)] public float phase1SuccessBias = 0.75f;

    [Header("Debug")]
    [SerializeField] private float thresholdMin;
    [SerializeField] private float thresholdMax;
    [SerializeField] private float normalizedTimerRemaining;

    private ISignalProvider userSignal;
    private IResettableSignal userReset;
    private float timer;

    private void Awake()
    {
        userSignal = userSignalProviderBehaviour as ISignalProvider;
        userReset = userSignalProviderBehaviour as IResettableSignal;
    }

    private void OnEnable()
    {
        timer = 0f;
        charge = 0f;

        phase1Gimmick?.ResetSignal();
        userReset?.ResetSignal();

        if (ui != null)
        {
            ui.SetNeuroValue(0f);
            ui.SetChargeValue(0f);
        }

        if (pillarMeter3D != null)
        {
            pillarMeter3D.SetNeuroValueImmediate(0f);
            pillarMeter3D.SetChargeValueImmediate(0f);
        }

        if (barrelFeedback != null)
            barrelFeedback.SetChargeImmediate(0f);

        if (neuroBar3D != null)
        {
            neuroBar3D.SetNeuroValue(0f);
            neuroBar3D.SetChargeValue(0f);
            neuroBar3D.SetValueImmediate(0f, true);
            neuroBar3D.SetValueImmediate(0f, false);
        }

        RestartParticleTimer();

        UpdateTimerUI();
        UpdateNormalizedTimerDebug();
    }

    private void Start()
    {
        RandomizeThreshold();
        UpdateTimerUI();
        UpdateNormalizedTimerDebug();
    }

    private void Update()
    {
        if (phaseManager == null) return;

        timer += Time.deltaTime;

        float signal = 0f;

        if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
        {
            bool shouldRise = Random.value < phase1SuccessBias;

            if (phase1Gimmick != null && phase1Gimmick.signal > thresholdMin + 0.15f)
                shouldRise = Random.value < 0.45f;

            if (phase1Gimmick != null)
            {
                phase1Gimmick.Tick(shouldRise);
                signal = phase1Gimmick.GetSignal01();
            }
        }
        else
        {
            signal = (userSignal != null) ? userSignal.GetSignal01() : 0f;
        }

        signal = Mathf.Clamp01(signal);

        if (signal >= thresholdMin && signal <= thresholdMax)
            charge += chargeRate * Time.deltaTime;

        charge = Mathf.Clamp01(charge);

        if (ui != null)
        {
            ui.SetNeuroValue(signal);
            ui.SetChargeValue(charge);
        }

        if (pillarMeter3D != null)
        {
            pillarMeter3D.SetNeuroValue(signal);
            pillarMeter3D.SetChargeValue(charge);
        }

        if (barrelFeedback != null)
            barrelFeedback.SetChargeValue(charge);

        if (neuroBar3D != null)
        {
            neuroBar3D.SetNeuroValue(signal);
            neuroBar3D.SetChargeValue(charge);

            if (syncWithUI)
            {
                bool showNeuro = phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline;
                neuroBar3D.SetShowNeuroFill(showNeuro);
            }
        }

        UpdateTimerUI();
        UpdateNormalizedTimerDebug();

        if (timer >= sessionDuration)
        {
            Fire();

            timer = 0f;
            charge = 0f;

            if (ui != null)
                ui.SetChargeValue(0f);

            if (pillarMeter3D != null)
                pillarMeter3D.SetChargeValueImmediate(0f);

            if (barrelFeedback != null)
                barrelFeedback.SetChargeImmediate(0f);

            if (neuroBar3D != null)
            {
                neuroBar3D.SetChargeValue(0f);
                neuroBar3D.SetValueImmediate(0f, false);
            }

            if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
                phase1Gimmick?.ResetSignal();

            RandomizeThreshold();
            RestartParticleTimer();
        }
    }

    private void RestartParticleTimer()
    {
        if (particleDisintegrateTimer == null)
            return;

        particleDisintegrateTimer.duration = sessionDuration;
        particleDisintegrateTimer.StartTimer();
    }

    private void UpdateTimerUI()
    {
        if (shotTimerText == null) return;

        float remaining = Mathf.Clamp(sessionDuration - timer, 0f, sessionDuration);
        shotTimerText.text = remaining.ToString("F1") + "s";
    }

    private void UpdateNormalizedTimerDebug()
    {
        if (sessionDuration <= 0f)
        {
            normalizedTimerRemaining = 0f;
            return;
        }

        normalizedTimerRemaining = 1f - (timer / sessionDuration);
        normalizedTimerRemaining = Mathf.Clamp01(normalizedTimerRemaining);
    }

    private void RandomizeThreshold()
    {
        float center = Random.Range(bandCenterRange.x, bandCenterRange.y);
        thresholdMin = Mathf.Clamp01(center - bandSize * 0.5f);
        thresholdMax = Mathf.Clamp01(center + bandSize * 0.5f);

        if (ui != null)
            ui.SetThreshold(thresholdMin, thresholdMax);

        if (pillarMeter3D != null)
            pillarMeter3D.SetThreshold(thresholdMin, thresholdMax);

        if (neuroBar3D != null)
            neuroBar3D.SetThreshold(thresholdMin, thresholdMax);
    }

    private void Fire()
    {
        if (cannon == null)
            cannon = FindFirstObjectByType<NeuroCannonController>();

        if (cannon != null)
            cannon.Fire(charge);
    }

    public float GetCurrentNeuroValue()
    {
        if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
            return phase1Gimmick?.GetSignal01() ?? 0f;
        else
            return userSignal?.GetSignal01() ?? 0f;
    }

    public float GetCurrentCharge()
    {
        return charge;
    }

    public Vector2 GetCurrentThreshold()
    {
        return new Vector2(thresholdMin, thresholdMax);
    }

    public void ManualRandomizeThreshold()
    {
        RandomizeThreshold();
    }

    public void ResetCharge()
    {
        charge = 0f;

        if (ui != null)
            ui.SetChargeValue(0f);

        if (pillarMeter3D != null)
            pillarMeter3D.SetChargeValueImmediate(0f);

        if (barrelFeedback != null)
            barrelFeedback.SetChargeImmediate(0f);

        if (neuroBar3D != null)
        {
            neuroBar3D.SetChargeValue(0f);
            neuroBar3D.SetValueImmediate(0f, false);
        }
    }
}