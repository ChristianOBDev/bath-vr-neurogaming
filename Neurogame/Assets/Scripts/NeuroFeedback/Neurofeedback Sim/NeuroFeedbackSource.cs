using UnityEngine;

public class FakeNeurofeedbackSource : MonoBehaviour, INeuroSignal
{
    public enum MentalState { Relaxed, Focused, Stressed }

    [Header("State (simulate user)")]
    public MentalState state = MentalState.Relaxed;
    public bool allowKeyboardSwitch = true; // 1/2/3 changes state

    [Header("Feature Update Rate (Hz)")]
    [Range(5, 30)] public int featureHz = 15;

    [Header("Realism")]
    [Range(0.2f, 5f)] public float responsiveness = 1.2f; // low-pass speed
    [Range(0f, 1f)] public float noise = 0.18f;           // jitter
    [Range(0f, 0.2f)] public float drift = 0.02f;          // slow drift

    [Header("Quality / Dropouts")]
    [Range(0f, 1f)] public float baseQuality = 0.95f;
    [Range(0f, 1f)] public float dropoutChancePerSecond = 0.06f;
    public Vector2 dropoutDurationRange = new Vector2(0.2f, 1.0f);

    [Header("Artifact Spikes")]
    [Range(0f, 1f)] public float artifactChancePerSecond = 0.04f;
    public Vector2 artifactStrengthRange = new Vector2(0.2f, 0.8f);

    [Header("Debug")]
    public bool debugLogs = true;

    public float Alpha { get; private set; } = 1.0f;
    public float Beta { get; private set; } = 0.6f;
    public float Theta { get; private set; } = 0.7f;
    public float Quality { get; private set; } = 1.0f;

    private float nextTick;
    private float tickDt;

    private float alphaDrift, betaDrift, thetaDrift;
    private float dropoutTimer;
    private float artifactTimer;
    private float artifactBoost;

    private float dbgT;

    void Awake()
    {
        tickDt = 1f / Mathf.Max(1, featureHz);
        nextTick = Time.time;
        Quality = baseQuality;
    }

    void Update()
    {
        if (allowKeyboardSwitch)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) state = MentalState.Relaxed;
            if (Input.GetKeyDown(KeyCode.Alpha2)) state = MentalState.Focused;
            if (Input.GetKeyDown(KeyCode.Alpha3)) state = MentalState.Stressed;
        }

        if (Time.time >= nextTick)
        {
            nextTick += tickDt;
            StepFeatures(tickDt);
        }

        if (debugLogs)
        {
            dbgT += Time.deltaTime;
            if (dbgT >= 1f)
            {
                dbgT = 0f;
                Debug.Log($"[EEG] State:{state} | Alpha:{Alpha:F2} | Beta:{Beta:F2} | Theta:{Theta:F2} | Quality:{Quality:F2}");
            }
        }
    }

    private void StepFeatures(float dt)
    {
        // slow drift
        alphaDrift = Mathf.Clamp(alphaDrift + Random.Range(-drift, drift) * dt, -0.25f, 0.25f);
        betaDrift = Mathf.Clamp(betaDrift + Random.Range(-drift, drift) * dt, -0.25f, 0.25f);
        thetaDrift = Mathf.Clamp(thetaDrift + Random.Range(-drift, drift) * dt, -0.25f, 0.25f);

        // state targets
        float aTarget, bTarget, tTarget;
        switch (state)
        {
            case MentalState.Relaxed:
                aTarget = 1.25f; bTarget = 0.50f; tTarget = 0.75f;
                break;
            case MentalState.Focused:
                aTarget = 0.95f; bTarget = 0.75f; tTarget = 0.60f;
                break;
            default: // Stressed
                aTarget = 0.70f; bTarget = 0.95f; tTarget = 0.85f;
                break;
        }

        aTarget += alphaDrift;
        bTarget += betaDrift;
        tTarget += thetaDrift;

        // dropouts (quality dips)
        if (dropoutTimer <= 0f && Random.value < dropoutChancePerSecond * dt)
            dropoutTimer = Random.Range(dropoutDurationRange.x, dropoutDurationRange.y);

        if (dropoutTimer > 0f)
        {
            dropoutTimer -= dt;
            Quality = Mathf.Lerp(Quality, 0.25f, 1f - Mathf.Exp(-6f * dt));
        }
        else
        {
            Quality = Mathf.Lerp(Quality, baseQuality, 1f - Mathf.Exp(-2f * dt));
        }

        // artifact spikes
        if (artifactTimer <= 0f && Random.value < artifactChancePerSecond * dt)
        {
            artifactTimer = Random.Range(0.05f, 0.2f);
            artifactBoost = Random.Range(artifactStrengthRange.x, artifactStrengthRange.y);
        }
        if (artifactTimer > 0f) artifactTimer -= dt;

        float artifact = (artifactTimer > 0f) ? artifactBoost : 0f;

        // noisy observations
        float noiseScale = noise * Mathf.Lerp(1f, 2.5f, 1f - Quality);
        float aObs = aTarget + RandomGaussian() * noiseScale - artifact * 0.2f;
        float bObs = bTarget + RandomGaussian() * noiseScale + artifact * 0.5f;
        float tObs = tTarget + RandomGaussian() * noiseScale + artifact * 0.3f;

        // low-pass filter for realistic latency
        float k = 1f - Mathf.Exp(-responsiveness * dt);
        Alpha = Mathf.Lerp(Alpha, Mathf.Max(0.05f, aObs), k);
        Beta = Mathf.Lerp(Beta, Mathf.Max(0.05f, bObs), k);
        Theta = Mathf.Lerp(Theta, Mathf.Max(0.05f, tObs), k);
    }

    private float RandomGaussian()
    {
        float u1 = Mathf.Max(1e-6f, Random.value);
        float u2 = Mathf.Max(1e-6f, Random.value);
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }
}
