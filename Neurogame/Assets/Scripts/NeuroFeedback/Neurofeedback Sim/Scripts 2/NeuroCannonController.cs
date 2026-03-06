using UnityEngine;

public class NeuroCannonController : MonoBehaviour
{
    [Header("Projectile")]
    public Transform muzzle;
    public Rigidbody projectilePrefab;

    [Header("Targets")]
    public NeuroMultiTargetManager targetManager;

    [Header("Ballistics")]
    [Tooltip("Launch angle in degrees for the arc. 35–55 is usually nice.")]
    [Range(10f, 80f)] public float launchAngleDeg = 45f;

    [Tooltip("Extra upward offset to aim at ship center mass.")]
    public float aimUpOffset = 0.8f;

    [Header("Accuracy / Spread")]
    public float maxSpreadDegrees = 6f;

    [Header("Damage Scaling")]
    public float minDamage = 20f;
    public float maxDamage = 120f;

    [Header("Spawn Safety")]
    public float spawnForwardOffset = 1.0f;
    public Collider[] cannonCollidersToIgnore;

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

        // Aim point: collider center (best) + small up offset
        Vector3 aimPoint = GetBestAimPoint(target) + Vector3.up * aimUpOffset;

        // Add spread based on charge (more charge = less spread)
        float spread = (1f - charge01) * maxSpreadDegrees;
        Vector3 spreadEuler = new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        Vector3 flatToTarget = aimPoint - muzzle.position;
        Vector3 dirNoSpread = flatToTarget.normalized;
        Vector3 dir = Quaternion.Euler(spreadEuler) * dirNoSpread;

        // Spawn slightly forward along current dir
        Vector3 spawnPos = muzzle.position + dir * spawnForwardOffset;

        Rigidbody rb = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Make sure physics is active + uses gravity for arc
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.angularVelocity = Vector3.zero;

        // Set projectile damage based on charge
        NeuroPowerProjectile proj = rb.GetComponent<NeuroPowerProjectile>();
        if (proj != null)
            proj.damage = Mathf.Lerp(minDamage, maxDamage, charge01);

        // Ignore cannon collisions so it doesn't trigger immediately at spawn
        Collider projCol = rb.GetComponent<Collider>();
        if (projCol != null)
        {
            if (cannonCollidersToIgnore != null)
            {
                foreach (var c in cannonCollidersToIgnore)
                    if (c != null) Physics.IgnoreCollision(projCol, c, true);
            }

            var parentCols = muzzle.GetComponentsInParent<Collider>(true);
            foreach (var c in parentCols)
                if (c != null) Physics.IgnoreCollision(projCol, c, true);
        }

        // Ballistic velocity solve at fixed angle
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
            // Fallback: if angle solve fails, just shoot toward target
            // (This can happen if target is too close/high for the chosen angle)
            float fallbackSpeed = 35f;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * fallbackSpeed;
#else
            rb.velocity = dir * fallbackSpeed;
#endif
            Debug.LogWarning("[NeuroCannonController] Ballistic solve failed; using fallback straight shot.");
        }

        PhaseGatedContinuousScore.Instance?.OnShotFired(charge01, charge01 > 0.7f, charge01);
    }

    private Vector3 GetBestAimPoint(NeuroTargetHealth target)
    {
        Collider col = target.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.center;
        return target.transform.position;
    }

    /// <summary>
    /// Computes an initial velocity to hit target using a fixed launch angle (degrees) under gravity.
    /// Returns false if the solution is not valid (e.g. angle too low/high for geometry).
    /// </summary>
    private bool TryGetBallisticVelocity(Vector3 start, Vector3 target, float angleDeg, out Vector3 velocity)
    {
        velocity = Vector3.zero;

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        float x = toTargetXZ.magnitude; // horizontal distance
        float y = toTarget.y;           // vertical distance

        if (x < 0.01f) return false;

        float g = Physics.gravity.magnitude;
        float angleRad = angleDeg * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        // v^2 = (g*x^2) / (2*cos^2*(x*tan - y))
        float denom = 2f * cos * cos * (x * Mathf.Tan(angleRad) - y);
        if (denom <= 0.0001f) return false;

        float v2 = (g * x * x) / denom;
        if (v2 <= 0f) return false;

        float v = Mathf.Sqrt(v2);

        // Build velocity vector: horizontal in XZ + vertical
        Vector3 dirXZ = toTargetXZ.normalized;

        velocity = dirXZ * (v * cos) + Vector3.up * (v * sin);
        return true;
    }
}