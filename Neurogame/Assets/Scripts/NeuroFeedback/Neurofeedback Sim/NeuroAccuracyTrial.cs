using UnityEngine;
using NeuroFeedback;    
public class NeuroAccuracyTrial : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PhaseManager phaseManager;
    [SerializeField] private NeuroMultiTargetManager targetManager;
    [SerializeField] private NeuroBandTrainer trainer;

    [SerializeField] private Transform muzzle;
    [SerializeField] private Rigidbody projectilePrefab;

    [Header("Trial Timing")]
    public float trialSeconds = 30f;
    public bool autoLoopTrials = true;
    public bool resetChargeOnTrialStart = true;

    private float t;
    private bool running;

    private float sumInBand;
    private int samples;

    [Header("Ballistics")]
    public float flightTime = 1.2f;
    public float aimHeightOffset = 1.0f;

    [Header("Accuracy Mapping (from charge01)")]
    public float maxSpreadDegrees = 8f;
    public float minSpreadDegrees = 0.4f;
    [Range(0f, 1f)] public float minAimAssist = 0.20f;
    [Range(0f, 1f)] public float maxAimAssist = 0.90f;

    void Start()
    {
        if (autoLoopTrials) StartTrial();
    }

    void Update()
    {
        if (!running || trainer == null || phaseManager == null) return;

        t += Time.deltaTime;
        sumInBand += trainer.inBand01;
        samples++;

        if (t >= trialSeconds)
            EndTrialAndFire();
    }

    public void StartTrial()
    {
        if (trainer == null || phaseManager == null) return;

        running = true;
        t = 0f;
        sumInBand = 0f;
        samples = 0;

        if (resetChargeOnTrialStart)
            trainer.charge01 = 0f;

        // ✅ Refresh threshold EVERY trial (every 30s)
        trainer.SetNewBandForTrial(phaseManager.CurrentPhase, trainer.brain01);
    }

    private void EndTrialAndFire()
    {
        running = false;

        float avgInBand = (samples > 0) ? (sumInBand / samples) : 0f;
        avgInBand = Mathf.Clamp01(avgInBand);

        trainer.ReportTrialResult(avgInBand);

        float charge = Mathf.Clamp01(trainer.charge01);

        // Fire one projectile with accuracy based on charge
        FireWithChargeAccuracy(charge);

        if (autoLoopTrials)
            StartTrial();
    }

    private void FireWithChargeAccuracy(float charge01)
    {
        if (muzzle == null || projectilePrefab == null || targetManager == null) return;

        var target = targetManager.GetCurrentAliveTarget();
        if (target == null) return;

        Vector3 targetPos = target.transform.position + Vector3.up * aimHeightOffset;
        Vector3 perfectV = ComputeBallisticVelocity(muzzle.position, targetPos, flightTime);

        float spread = Mathf.Lerp(maxSpreadDegrees, minSpreadDegrees, charge01);
        float assist = Mathf.Lerp(minAimAssist, maxAimAssist, charge01);

        Vector3 spreadV = ApplySpread(perfectV, spread);
        Vector3 finalDir = Vector3.Slerp(spreadV.normalized, perfectV.normalized, assist);
        Vector3 finalV = finalDir * perfectV.magnitude;

        Rigidbody rb = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(finalV.normalized, Vector3.up));
        rb.linearVelocity = finalV;
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