using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlockerRepulsor : MonoBehaviour
{
    [Header("Force")]
    public float repulsionForce = 1f;
    public float upwardForce = 2f;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        Vector3 radial = (rb.position - transform.position).normalized;
        Vector3 force = radial * repulsionForce + Vector3.up * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);
    }
}