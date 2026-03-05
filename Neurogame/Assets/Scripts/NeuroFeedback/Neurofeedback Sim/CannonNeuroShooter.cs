using UnityEngine;
using NeuroFeedback;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CannonNeuroShooterAutoAim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private NeuroMultiTargetManager targetManager;
    [SerializeField] private NeuroFeedbackController neuro;
    [SerializeField] private NeuroMeter meter;

    [SerializeField] private Transform muzzle;
    [SerializeField] private Rigidbody projectilePrefab;

    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Space;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference triggerAction;
    private bool triggerHeld;
#endif

    [Header("Ballistic Aim")]
    public float flightTime = 1.2f;
    public float aimHeightOffset = 1.0f;

    [Header("Phase 1 Auto Fire")]
    [Tooltip("Seconds between shots in Phase 1.")]
    public float phase1AutoFireInterval = 1.2f;

    [Header("Phase 2/3 Focus Window")]
    [Tooltip("Seconds between shots in Phase 2/3 (gives user time to focus).")]
    public float timeBetweenShotsPhase23 = 30f;

    [Tooltip("If true, cannon auto-fires as soon as ready + locked stable in Phase 2/3.")]
    public bool autoFireWhenReady = false;

    [Header("Spread/Wobble")]
    public Transform aimPivot;
    public float maxWobbleDegrees = 6f;
    public float maxSpreadDegrees = 6f;
    public float minSpreadDegrees = 0.5f;

    [Header("Aim Assist (Phase 2/3)")]
    [Tooltip("Blend toward perfect aim in Phase 2 (0=no assist, 1=perfect).")]
    [Range(0f, 1f)] public float phase2AimAssist = 0.65f;

    [Tooltip("Blend toward perfect aim in Phase 3 (0=no assist, 1=perfect).")]
    [Range(0f, 1f)] public float phase3AimAssist = 0.35f;

    [Tooltip("Extra assist gained from stabilityScore (0..1).")]
    [Range(0f, 1f)] public float stabilityAssistGain = 0.35f;

    [Header("Manual Aim Assist (optional testing)")]
    [Range(0f, 1f)] public float manualAimAssist01 = 0f;

    [Header("Optimal Band Accuracy Boost")]
    [Tooltip("When meter is in green band, spread is multiplied by this (smaller = more accurate).")]
    [Range(0.01f, 1f)] public float optimalSpreadMultiplier = 0.25f;

    [Tooltip("If true, you only get the accuracy boost when OptimalReady is true.")]
    public bool requireOptimalHold = false;

    [Header("UI Hit Chance (0..1)")]
    [Range(0f, 1f)] public float hitChance01 = 0f;

    [Header("Debug")]
    public bool debugLogs = true;

    private float wobblePhase;

    // Unified cooldown used in all phases (Phase 1 uses phase1 interval, Phase 2/3 uses 30s)
    private float shotCooldown;

#if ENABLE_INPUT_SYSTEM
    void OnEnable()
    {
        if (triggerAction != null && triggerAction.action != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.started += _ => triggerHeld = true;
            triggerAction.action.canceled += _ => triggerHeld = false;
        }
    }

    void OnDisable()
    {
        if (triggerAction != null && triggerAction.action != null)
            triggerAction.action.Disable();
    }
#endif

    void Update()
    {
        if (phaseManager == null || muzzle == null || projectilePrefab == null) return;
        if (targetManager == null) return;

        UpdateWobble();

        if (shotCooldown > 0f)
            shotCooldown -= Time.deltaTime;

        var phase = phaseManager.CurrentPhase;

        // -----------------------------
        // Phase 1: auto-fire baseline
        // -----------------------------
        if (phase == PhaseManager.Phase.Phase1_Baseline)
        {
            if (shotCooldown > 0f) return;

            FireAtCurrentTarget();              // fires once
            shotCooldown = phase1AutoFireInterval; // then waits
            return;
        }

        // -----------------------------------------
        // Phase 2/3: one shot per focus window
        // -----------------------------------------
        bool pressed = Input.GetKeyDown(fireKey)
#if ENABLE_INPUT_SYSTEM
                       || triggerHeld
#endif
                       ;

        bool ready = shotCooldown <= 0f;
        bool locked = (neuro != null) ? neuro.IsLockedStable : true;

        bool canFireNow = ready && locked;

        if (autoFireWhenReady)
        {
            if (canFireNow)
                FireAtCurrentTarget();
        }
        else
        {
            if (pressed && canFireNow)
                FireAtCurrentTarget();
            else if (pressed && !canFireNow && debugLogs)
                Debug.Log($"[CANNON] Can't fire. ready:{ready} locked:{locked} cooldown:{Mathf.Max(0f, shotCooldown):F1}s phase:{phase}");
        }
    }

    private void FireAtCurrentTarget()
    {
        NeuroTargetHealth target = targetManager.GetCurrentAliveTarget();
        if (target == null)
        {
            if (debugLogs) Debug.LogWarning("[CANNON] No alive target found.");
            return;
        }

        Vector3 targetPos = target.transform.position + Vector3.up * aimHeightOffset;

        // Perfect ballistic velocity
        Vector3 perfectV = ComputeBallisticVelocity(muzzle.position, targetPos, flightTime);

        float stab = (neuro != null) ? Mathf.Clamp01(neuro.stabilityScore) : 0.7f;

        // Spread from stability
        float spread = Mathf.Lerp(maxSpreadDegrees, minSpreadDegrees, stab);

        // Optimal band reduces spread
        bool optimal = false;
        if (meter != null)
        {
            optimal = requireOptimalHold ? meter.OptimalReady : meter.InOptimalBand;
            if (optimal)
                spread *= optimalSpreadMultiplier;
        }

        // Apply spread first
        Vector3 spreadV = ApplySpread(perfectV, spread);

        // Aim assist blend (Phase2 > Phase3)
        float baseAssist = (phaseManager.CurrentPhase == PhaseManager.Phase.Phase2_Assisted) ? phase2AimAssist : phase3AimAssist;
        float assist = Mathf.Clamp01(baseAssist + stab * stabilityAssistGain + manualAimAssist01);

        Vector3 finalDir = Vector3.Slerp(spreadV.normalized, perfectV.normalized, assist);
        Vector3 finalV = finalDir * perfectV.magnitude;

        // UI hit chance estimate
        float spreadFactor = Mathf.InverseLerp(maxSpreadDegrees, minSpreadDegrees, Mathf.Clamp(spread, minSpreadDegrees, maxSpreadDegrees));
        float meterFactor = (meter != null) ? meter.meterValue : 0.6f;
        float optimalFactor = optimal ? 1f : 0.6f;

        hitChance01 = Mathf.Clamp01(
            (0.40f * spreadFactor) +
            (0.25f * stab) +
            (0.15f * meterFactor) +
            (0.20f * assist)
        ) * optimalFactor;

        // Spawn projectile + shoot
        Rigidbody rb = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(finalV.normalized, Vector3.up));
        rb.linearVelocity = finalV;

        // Set cooldown depending on phase
        if (phaseManager.CurrentPhase == PhaseManager.Phase.Phase1_Baseline)
            shotCooldown = phase1AutoFireInterval;
        else
            shotCooldown = timeBetweenShotsPhase23;

        // Optional: reward “good shot” attempt (Phase-gated inside score manager)
        PhaseGatedContinuousScore.Instance?.OnShotFired(stab, optimal, hitChance01);

        if (debugLogs)
            Debug.Log($"[CANNON] Fired. phase:{phaseManager.CurrentPhase} stab:{stab:F2} assist:{assist:F2} spread:{spread:F2} optimal:{optimal} chance:{hitChance01:F2} next:{shotCooldown:F0}s");
    }

    private void UpdateWobble()
    {
        if (aimPivot == null) return;

        float stab = (neuro != null) ? Mathf.Clamp01(neuro.stabilityScore) : 0.7f;
        float wobbleAmp = Mathf.Lerp(maxWobbleDegrees, 0.3f, stab);

        wobblePhase += Time.deltaTime * Mathf.Lerp(2.5f, 0.8f, stab);
        float wobble = Mathf.Sin(wobblePhase) * wobbleAmp;

        Vector3 e = aimPivot.localEulerAngles;
        e.y = wobble;
        aimPivot.localEulerAngles = e;
    }

    private Vector3 ComputeBallisticVelocity(Vector3 start, Vector3 end, float t)
    {
        Vector3 to = end - start;
        Vector3 toXZ = new Vector3(to.x, 0f, to.z);

        Vector3 vXZ = toXZ / Mathf.Max(0.01f, t);
        float vY = (to.y - 0.5f * Physics.gravity.y * t * t) / Mathf.Max(0.01f, t);

        return vXZ + Vector3.up * vY;
    }

    private Vector3 ApplySpread(Vector3 velocity, float degrees)
    {
        if (degrees <= 0.001f) return velocity;

        Quaternion q = Quaternion.Euler(Random.Range(-degrees, degrees), Random.Range(-degrees, degrees), 0f);
        return q * velocity;
    }
}