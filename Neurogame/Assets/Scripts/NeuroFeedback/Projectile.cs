using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PowerProjectile : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float maxLife = 10f;

    [Header("Hit VFX")]
    [SerializeField] private GameObject hitVfxPrefab; // explosion prefab on contact
    [SerializeField] private float hitVfxLife = 3f;
    [SerializeField] private float hitCooldown = 0.05f;

    private CannonShooter3Power.ShotType type;
    private float damage;

    private MultiTargetManager manager;
    private CannonShooter3Power cannon;
    private Rigidbody rb;

    private int targetsHitCount = 0;
    private TargetHealth lastTargetHit = null;
    private float lastHitTime = -999f;

    public void Init(CannonShooter3Power.ShotType shotType, float dmg, MultiTargetManager m, CannonShooter3Power cannonRef)
    {
        type = shotType;
        damage = dmg;
        manager = m;
        cannon = cannonRef;

        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLife);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        // Ships usually have colliders on children
        TargetHealth th = col.collider.GetComponentInParent<TargetHealth>();
        if (th == null) return;
        if (!th.IsAlive) return;

        if (th == lastTargetHit) return;

        SpawnHitVFX(col);

        th.ApplyDamage(damage);

        lastTargetHit = th;
        targetsHitCount++;

        if (type == CannonShooter3Power.ShotType.MidChain)
        {
            if (targetsHitCount == 1)
            {
                if (manager != null && cannon != null && rb != null)
                {
                    TargetHealth next = manager.GetNearestAliveTarget(transform.position, th);
                    if (next != null)
                    {
                        Vector3 v1 = cannon.ComputeBallisticVelocity(transform.position, next.transform.position, 1.0f);
                        rb.linearVelocity = v1;
                        if (v1.sqrMagnitude > 0.001f) transform.forward = v1.normalized;
                        return; // continue to second target
                    }
                }

                Destroy(gameObject);
                return;
            }

            if (targetsHitCount >= 2)
            {
                Destroy(gameObject);
                return;
            }

            return;
        }

        Destroy(gameObject);
    }

    private void SpawnHitVFX(Collision col)
    {
        if (hitVfxPrefab == null) return;

        Vector3 pos = transform.position;
        Quaternion rot = Quaternion.identity;

        if (col.contactCount > 0)
        {
            var cp = col.GetContact(0);
            pos = cp.point;
            rot = Quaternion.LookRotation(cp.normal);
        }

        GameObject vfx = Instantiate(hitVfxPrefab, pos, rot);
        Destroy(vfx, hitVfxLife);
    }
}
