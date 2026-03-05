using UnityEngine;
using TMPro;
using NeuroFeedback;

public class NeuroChargeController : MonoBehaviour
{
    [Header("Signal Providers")]
    public SlowGimmickSignalProvider phase1Gimmick;
    public MonoBehaviour userSignalProviderBehaviour;

    [Header("Refs")]
    public NeuroMeterUI ui;
    public PhaseManager phaseManager;
    public NeuroCannonController cannon;

    [Header("Session")]
    public float sessionDuration = 30f;

    [Header("UI Timer (TextMeshPro)")]
    [Tooltip("Drop a TextMeshProUGUI element here to show the countdown until the cannon fires.")]
    public TextMeshPro shotTimerText;

    [Header("Threshold Band")]
    [Range(0.02f, 0.5f)] public float bandSize = 0.18f;
    public Vector2 bandCenterRange = new Vector2(0.25f, 0.85f);

    [Header("Charge (fills only)")]
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

        if (ui != null)
        {
            ui.SetNeuroValue(0f);
            ui.SetChargeValue(0f);
        }

        UpdateTimerUI();
    }

    private void Start()
    {
        RandomizeThreshold();
    }

    void Update()
    {
        if (ui == null || phaseManager == null) return;

        timer += Time.deltaTime;

        float signal;

        if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
        {
            bool shouldRise = (Random.value < phase1SuccessBias);

            if (phase1Gimmick != null && phase1Gimmick.signal > thresholdMin + 0.15f)
                shouldRise = (Random.value < 0.45f);

            phase1Gimmick.Tick(shouldRise);
            signal = phase1Gimmick.GetSignal01();
        }
        else
        {
            signal = (userSignal != null) ? userSignal.GetSignal01() : 0f;
        }

        signal = Mathf.Clamp01(signal);

        if (signal >= thresholdMin)
            charge += chargeRate * Time.deltaTime;

        charge = Mathf.Clamp01(charge);

        ui.SetNeuroValue(signal);
        ui.SetChargeValue(charge);

        UpdateTimerUI();

        if (timer >= sessionDuration)
        {
            Fire();

            timer = 0f;
            charge = 0f;
            ui.SetChargeValue(0f);

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

        ui.SetThreshold(thresholdMin, thresholdMax);
    }

    private void Fire()
    {
        if (cannon == null)
            cannon = FindFirstObjectByType<NeuroCannonController>();

        if (cannon != null)
            cannon.Fire(charge);
    }
}