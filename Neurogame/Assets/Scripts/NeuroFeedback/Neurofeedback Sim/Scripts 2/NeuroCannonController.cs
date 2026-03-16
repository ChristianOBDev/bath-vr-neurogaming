using UnityEngine;

public class NeuroCannonController : MonoBehaviour
{
    [Header("Projectile")]
    public Transform muzzle;
    public Rigidbody projectilePrefab;

    [Header("Turret")]
    [Tooltip("The rotating turret/base transform. This rotates only on Y axis.")]
    public Transform turretTransform;

    [Tooltip("How fast the turret rotates toward the target.")]
    public float turretTurnSpeed = 8f;

    [Header("Targets")]
    public NeuroMultiTargetManager targetManager;

    [Header("Ballistics")]
    [Range(10f, 80f)] public float launchAngleDeg = 45f;
    public float aimUpOffset = 0.8f;

    [Header("Accuracy / Spread")]
    public float maxSpreadDegrees = 6f;

    [Header("Damage Scaling")]
    public float minDamage = 20f;
    public float maxDamage = 120f;

    [Header("Spawn Safety")]
    public float spawnForwardOffset = 1.0f;
    public Collider[] cannonCollidersToIgnore;

    [Header("Shot Feedback")]
    public ParticleSystem shotEffect;
    public AudioSource shotAudioSource;

    private void Awake()
    {
        if (shotEffect != null)
        {
            shotEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {
        if (targetManager == null || turretTransform == null)
            return;

        NeuroTargetHealth target = targetManager.GetCurrentAliveTarget();
        if (target == null)
            return;

        Vector3 aimPoint = GetBestAimPoint(target) + Vector3.up * aimUpOffset;
        AimTurretAt(aimPoint, false);
    }

    public void Fire(float charge01)
    {
        if (!muzzle || !projectilePrefab || !targetManager)
        {
            Debug.LogWarning("[NeuroCannonController] Missing references.");
            return;
        }

        NeuroTargetHealth target = targetManager.GetCurrentAliveTarget();
        if (target == null) return;

        charge01 = Mathf.Clamp01(charge01);

        Vector3 aimPoint = GetBestAimPoint(target) + Vector3.up * aimUpOffset;

        // Snap turret to target just before firing
        if (turretTransform != null)
            AimTurretAt(aimPoint, true);

        float spread = (1f - charge01) * maxSpreadDegrees;
        Vector3 spreadEuler = new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        Vector3 flatToTarget = aimPoint - muzzle.position;
        Vector3 dirNoSpread = flatToTarget.normalized;
        Vector3 dir = Quaternion.Euler(spreadEuler) * dirNoSpread;

        Vector3 spawnPos = muzzle.position + dir * spawnForwardOffset;

        Rigidbody rb = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.angularVelocity = Vector3.zero;

        NeuroPowerProjectile proj = rb.GetComponent<NeuroPowerProjectile>();
        if (proj != null)
            proj.damage = Mathf.Lerp(minDamage, maxDamage, charge01);

        Collider projCol = rb.GetComponent<Collider>();
        if (projCol != null)
        {
            if (cannonCollidersToIgnore != null)
            {
                foreach (var c in cannonCollidersToIgnore)
                {
                    if (c != null)
                        Physics.IgnoreCollision(projCol, c, true);
                }
            }

            var parentCols = muzzle.GetComponentsInParent<Collider>(true);
            foreach (var c in parentCols)
            {
                if (c != null)
                    Physics.IgnoreCollision(projCol, c, true);
            }
        }

        if (TryGetBallisticVelocity(spawnPos, aimPoint, launchAngleDeg, out Vector3 v0))
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v0;
#else
            rb.velocity = v0;
#endif
        }
        else
        {
            float fallbackSpeed = 35f;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * fallbackSpeed;
#else
            rb.velocity = dir * fallbackSpeed;
#endif
            Debug.LogWarning("[NeuroCannonController] Ballistic solve failed; using fallback shot.");
        }

        PlayShotFeedback();

        PhaseGatedContinuousScore.Instance?.OnShotFired(charge01, charge01 > 0.7f, charge01);
    }

    private void AimTurretAt(Vector3 worldPoint, bool instant = false)
    {
        if (turretTransform == null)
            return;

        Vector3 dir = worldPoint - turretTransform.position;

        // Y-axis only rotation
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        if (instant)
        {
            turretTransform.rotation = targetRot;
        }
        else
        {
            turretTransform.rotation = Quaternion.Slerp(
                turretTransform.rotation,
                targetRot,
                turretTurnSpeed * Time.deltaTime
            );
        }
    }

    private void PlayShotFeedback()
    {
        if (shotEffect != null)
        {
            shotEffect.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
            shotEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            shotEffect.Play(true);
        }

        if (shotAudioSource != null)
        {
            shotAudioSource.Stop();
            shotAudioSource.Play();
        }
    }

    private Vector3 GetBestAimPoint(NeuroTargetHealth target)
    {
        Collider col = target.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.center;
        return target.transform.position;
    }

    private bool TryGetBallisticVelocity(Vector3 start, Vector3 target, float angleDeg, out Vector3 velocity)
    {
        velocity = Vector3.zero;

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude;
        float y = toTarget.y;

        if (x < 0.01f) return false;

        float g = Physics.gravity.magnitude;
        float angleRad = angleDeg * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angleRad);
        float denom = 2f * cos * cos * (x * Mathf.Tan(angleRad) - y);
        if (denom <= 0.0001f) return false;

        float v2 = (g * x * x) / denom;
        if (v2 <= 0f) return false;

        float v = Mathf.Sqrt(v2);
        Vector3 dirXZ = toTargetXZ.normalized;

        velocity = dirXZ * (v * cos) + Vector3.up * (v * Mathf.Sin(angleRad));
        return true;
    }
}