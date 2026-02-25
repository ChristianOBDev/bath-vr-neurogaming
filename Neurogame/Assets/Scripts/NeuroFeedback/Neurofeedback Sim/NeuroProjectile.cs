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

    void Start()
    {
        Destroy(gameObject, maxLife);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (hitVfxPrefab != null && col.contactCount > 0)
        {
            Instantiate(hitVfxPrefab, col.contacts[0].point, Quaternion.identity);
        }

        var th = col.collider.GetComponentInParent<NeuroTargetHealth>();
        if (th != null && th.IsAlive)
        {
            th.ApplyDamage(damage);
        }

        Destroy(gameObject);
    }
}