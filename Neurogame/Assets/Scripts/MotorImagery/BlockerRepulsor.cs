using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlockerRepulsor : MonoBehaviour
{
    [Header("Force")]
    public float repulsionForce = 10f;
    public float upwardForce = 5f;

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        Vector3 upAxis = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Up
            : Vector3.up;

        Vector3 radial = (rb.position - transform.position).normalized;
        Vector3 force = radial * repulsionForce + upAxis * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);
    }
}