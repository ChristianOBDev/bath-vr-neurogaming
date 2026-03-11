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
    public NeuroBar3D neuroBar3D; // Reference to the 3D bar
    public bool syncWithUI = true; // Whether to sync 3D bar with UI values

    [Header("Phase Manager")]
    public PhaseManager phaseManager;

    [Header("Session")]
    public float sessionDuration = 30f;

    [Header("Timer (3D TextMeshPro)")]
    public TextMeshPro shotTimerText;

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

        // Reset UI elements
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

        // Reset 3D bar
        if (neuroBar3D != null)
        {
            neuroBar3D.SetNeuroValue(0f);
            neuroBar3D.SetChargeValue(0f);
            neuroBar3D.SetValueImmediate(0f, true); // Immediate neuro reset
            neuroBar3D.SetValueImmediate(0f, false); // Immediate charge reset
        }

        UpdateTimerUI();
    }

    private void Start()
    {
        RandomizeThreshold();
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

        // Update charge based on threshold
        if (signal >= thresholdMin && signal <= thresholdMax)
            charge += chargeRate * Time.deltaTime;

        charge = Mathf.Clamp01(charge);

        // Update UI elements
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

        // Update 3D bar
        if (neuroBar3D != null)
        {
            neuroBar3D.SetNeuroValue(signal);
            neuroBar3D.SetChargeValue(charge);
            
            // Optionally sync which value is displayed
            if (syncWithUI)
            {
                // You can control which value the 3D bar shows based on phase
                bool showNeuro = phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline;
                neuroBar3D.SetShowNeuroFill(showNeuro);
            }
        }

        UpdateTimerUI();

        if (timer >= sessionDuration)
        {
            Fire();

            timer = 0f;
            charge = 0f;

            // Reset UI elements
            if (ui != null)
                ui.SetChargeValue(0f);

            if (pillarMeter3D != null)
                pillarMeter3D.SetChargeValueImmediate(0f);

            if (barrelFeedback != null)
                barrelFeedback.SetChargeImmediate(0f);

            // Reset 3D bar
            if (neuroBar3D != null)
            {
                neuroBar3D.SetChargeValue(0f);
                neuroBar3D.SetValueImmediate(0f, false); // Immediate charge reset
            }

            if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
                phase1Gimmick?.ResetSignal();

            RandomizeThreshold();
        }
    }

    private void UpdateTimerUI()
    {
        if (shotTimerText == null) return;

        float remaining = Mathf.Clamp(sessionDuration - timer, 0f, sessionDuration);
        shotTimerText.text = remaining.ToString("F1") + "s";
    }

    private void RandomizeThreshold()
    {
        float center = Random.Range(bandCenterRange.x, bandCenterRange.y);
        thresholdMin = Mathf.Clamp01(center - bandSize * 0.5f);
        thresholdMax = Mathf.Clamp01(center + bandSize * 0.5f);

        // Update UI thresholds
        if (ui != null)
            ui.SetThreshold(thresholdMin, thresholdMax);

        if (pillarMeter3D != null)
            pillarMeter3D.SetThreshold(thresholdMin, thresholdMax);

        // Update 3D bar threshold
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

    // Public methods to get current values (useful for debugging)
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

    // Method to manually trigger threshold randomization
    public void ManualRandomizeThreshold()
    {
        RandomizeThreshold();
    }

    // Method to manually reset charge
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