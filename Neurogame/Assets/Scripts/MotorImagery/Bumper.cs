using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bumper : MonoBehaviour
{
    [Header("Force")]
    public float repulsionForce = 50f;
    public float upwardForce = 20f;

    [Tooltip("How long the bumper applies force before bursting")]
    public float forceDuration = 0.15f;

    [Header("Burst")]
    public bool burstOnContact = true;
    public GameObject burstEffect;

    private Rigidbody balloon;
    private float forceTimer;
    private bool activated;

    void OnCollisionEnter(Collision collision)
    {
        if (activated) return;

        if (collision.rigidbody != null)
        {
            balloon = collision.rigidbody;
            activated = true;
            forceTimer = forceDuration;
        }
    }

    void FixedUpdate()
    {
        if (!activated || balloon == null)
            return;

        Vector3 radial =
            (balloon.position - transform.position).normalized;

        Vector3 force =
            radial * repulsionForce +
            Vector3.up * upwardForce;

        balloon.AddForce(force, ForceMode.Force);

        forceTimer -= Time.fixedDeltaTime;

        if (forceTimer <= 0f)
        {
            Burst();
        }
    }

    void Burst()
    {
        if (burstEffect != null)
        {
            Instantiate(
                burstEffect,
                transform.position,
                Quaternion.identity
            );
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBumperDestroyed(this);
        }

        Destroy(gameObject);
    }
}
