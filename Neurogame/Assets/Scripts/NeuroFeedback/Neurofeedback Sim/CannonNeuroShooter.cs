using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CannonNeuroShooterAutoAim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private NeuroMultiTargetManager targetManager;   // <-- NEW: manager drives target selection
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
    public float phase1AutoFireInterval = 1.2f;

    [Header("Charge (Phase 2/3)")]
    public float chargeTime = 1.2f;
    public float minChargeToFire = 0.2f;
    private float charge;

    [Header("Spread/Wobble")]
    public Transform aimPivot;
    public float maxWobbleDegrees = 6f;
    public float maxSpreadDegrees = 6f;
    public float minSpreadDegrees = 0.5f;

    [Header("Reload")]
    public float baseReload = 0.7f;
    public float maxExtraReloadWhenUnstable = 1.0f;
    private float reloadTimer;

    [Header("Fallback")]
    public float fallbackSpreadMultiplier = 1.6f;

    [Header("Debug")]
    public bool debugLogs = true;

    private float wobblePhase;
    private float autoFireT;

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
        if (targetManager == null) return; // must be assigned

        if (reloadTimer > 0f) reloadTimer -= Time.deltaTime;

        UpdateWobble();

        var phase = phaseManager.CurrentPhase;

        // Phase 1: auto-fire
        if (phase == PhaseManager.Phase.Phase1_Baseline)
        {
            autoFireT += Time.deltaTime;
            if (autoFireT >= phase1AutoFireInterval)
            {
                autoFireT = 0f;
                FireAtCurrentTarget(locked: true);
            }
            return;
        }

        // Phase 2/3: hold to charge, release to fire
        bool holding = Input.GetKey(fireKey)
#if ENABLE_INPUT_SYSTEM
                       || triggerHeld
#endif
                       ;

        if (holding)
        {
            float stability = (neuro != null) ? neuro.stabilityScore : 0.7f;
            float m = (meter != null) ? meter.meterValue : 0.6f;

            float efficiency = Mathf.Lerp(0.8f, 1.35f, stability) * Mathf.Lerp(0.9f, 1.15f, m);
            charge += (Time.deltaTime / Mathf.Max(0.01f, chargeTime)) * efficiency;
            charge = Mathf.Clamp01(charge);
        }
        else
        {
            if (charge >= minChargeToFire)
            {
                bool locked = (neuro != null) ? neuro.IsLockedStable : true;
                FireAtCurrentTarget(locked);
            }
            charge = 0f;
        }
    }

    private void FireAtCurrentTarget(bool locked)
    {
        if (reloadTimer > 0f) return;

        NeuroTargetHealth target = targetManager.GetCurrentAliveTarget();
        if (target == null)
        {
            if (debugLogs) Debug.LogWarning("[CANNON] No alive target found.");
            return;
        }

        Vector3 targetPos = target.transform.position + Vector3.up * aimHeightOffset;

        Vector3 v = ComputeBallisticVelocity(muzzle.position, targetPos, flightTime);

        float stab = (neuro != null) ? neuro.stabilityScore : 0.7f;
        float spread = Mathf.Lerp(maxSpreadDegrees, minSpreadDegrees, stab);

        if (!locked)
            spread *= fallbackSpreadMultiplier;

        Rigidbody rb = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(v.normalized, Vector3.up));
        rb.linearVelocity = ApplySpread(v, spread);

        float extraReload = Mathf.Lerp(maxExtraReloadWhenUnstable, 0f, stab);
        reloadTimer = baseReload + extraReload;

        if (debugLogs)
            Debug.Log($"[CANNON] Fired -> target:{target.name} locked:{locked} spread:{spread:F2} phase:{phaseManager.CurrentPhase}");
    }

    private void UpdateWobble()
    {
        if (aimPivot == null) return;

        float stab = (neuro != null) ? neuro.stabilityScore : 0.7f;
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