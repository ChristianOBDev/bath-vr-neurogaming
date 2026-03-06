using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class NeuroPowerProjectile : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    [Header("Lifetime")]
    public float maxLife = 12f;

    [Header("Hit VFX (optional)")]
    public GameObject hitVfxPrefab;

    [Header("Spawn Safety")]
    public float armingDelay = 0.08f;

    private bool consumed;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, maxLife);

        var rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // IMPORTANT for triggers:
        // Projectile collider should NOT be trigger.
        // Ships are trigger, projectile is not.
        var col = GetComponent<Collider>();
        col.isTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHitShip(other, other.ClosestPoint(transform.position));
    }

    private void OnCollisionEnter(Collision col)
    {
        // If you also have non-trigger ship colliders, this still works
        TryHitShip(col.collider, col.contactCount > 0 ? col.contacts[0].point : transform.position);
    }

    private void TryHitShip(Collider hit, Vector3 hitPoint)
    {
        if (Time.time - spawnTime < armingDelay) return;
        if (consumed) return;

        // Only react to ships that have NeuroTargetHealth somewhere in parent chain
        var th = hit.GetComponentInParent<NeuroTargetHealth>();
        if (th == null || !th.IsAlive) return;

        consumed = true;

        if (hitVfxPrefab != null)
            Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);

        th.ApplyDamage(damage);
        PhaseGatedContinuousScore.Instance?.AddHit();

        Destroy(gameObject);
    }
}