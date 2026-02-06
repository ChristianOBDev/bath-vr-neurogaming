using System.Collections;
using UnityEngine;

public class CannonShooter3Power : MonoBehaviour
{
    public enum ShotType { LowSlow, MidChain, HighDoubleFast }

    [Header("Refs")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject projectilePrefab;   // prefab from Project
    [SerializeField] private MultiTargetManager targetManager;

    [Header("Ballistics (flight time maps speed by distance)")]
    [SerializeField] private float minFlightTime = 0.25f;
    [SerializeField] private float maxFlightTime = 1.0f;
    [SerializeField] private float distanceForMaxTime = 25f;

    [Header("Low (slow, low dmg)")]
    [SerializeField] private float lowDamage = 10f;
    [SerializeField] private float lowTimeMultiplier = 1.4f;

    [Header("Mid (chain: hit locked, then nearest)")]
    [SerializeField] private float midDamage = 18f;
    [SerializeField] private float midTimeMultiplier = 1.0f;

    [Header("High (fast, 2 shots)")]
    [SerializeField] private float highDamage = 30f;
    [SerializeField] private float highTimeMultiplier = 0.6f;
    [SerializeField] private float highSecondShotDelay = 0.12f;

    public void Fire(ShotType type)
    {
        if (muzzle == null || projectilePrefab == null || targetManager == null) return;
        if (targetManager.CurrentTarget == null) return;

        switch (type)
        {
            case ShotType.LowSlow:
                SpawnProjectile(type, targetManager.CurrentTarget, lowDamage, lowTimeMultiplier);
                break;

            case ShotType.MidChain:
                SpawnProjectile(type, targetManager.CurrentTarget, midDamage, midTimeMultiplier);
                break;

            case ShotType.HighDoubleFast:
                StartCoroutine(HighDoubleRoutine());
                break;
        }
    }

    private IEnumerator HighDoubleRoutine()
    {
        var t = targetManager.CurrentTarget;
        if (t == null) yield break;

        SpawnProjectile(ShotType.HighDoubleFast, t, highDamage, highTimeMultiplier);
        yield return new WaitForSeconds(highSecondShotDelay);

        // reacquire (maybe target changed)
        t = targetManager.CurrentTarget;
        if (t != null)
            SpawnProjectile(ShotType.HighDoubleFast, t, highDamage, highTimeMultiplier);
    }

    private void SpawnProjectile(ShotType type, TargetHealth target, float damage, float timeMultiplier)
    {
        if (target == null) return;

        GameObject go = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);

        Rigidbody rb = go.GetComponent<Rigidbody>();
        PowerProjectile proj = go.GetComponent<PowerProjectile>();

        if (rb == null || proj == null)
        {
            Destroy(go);
            return;
        }

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 v0 = ComputeBallisticVelocity(muzzle.position, target.transform.position, timeMultiplier);
        rb.linearVelocity = v0;

        proj.Init(type, damage, targetManager, this);
        if (v0.sqrMagnitude > 0.001f) go.transform.forward = v0.normalized;
    }

    public Vector3 ComputeBallisticVelocity(Vector3 start, Vector3 end, float timeMultiplier)
    {
        Vector3 to = end - start;
        Vector3 toXZ = new Vector3(to.x, 0f, to.z);
        float xzDist = toXZ.magnitude;

        float t01 = (distanceForMaxTime <= 0.001f) ? 1f : Mathf.Clamp01(xzDist / distanceForMaxTime);
        float tBase = Mathf.Lerp(minFlightTime, maxFlightTime, t01);
        float t = Mathf.Max(0.05f, tBase * timeMultiplier);

        float g = -Physics.gravity.y;

        Vector3 vxz = (xzDist < 0.0001f) ? Vector3.zero : toXZ / t;
        float vy = (to.y / t) + (0.5f * g * t);

        return vxz + Vector3.up * vy;
    }
}
