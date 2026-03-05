using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalloonFloatController : MonoBehaviour
{
    [Header("Buoyancy")]
    [Tooltip("Fraction of gravity to counteract")]
    [Range(0f, 1f)]
    public float buoyancyFactor = 0.7f;

    [Tooltip("Limit vertical speed")]
    public float maxVerticalSpeed = 5f;

    [Header("Air Drift")]
    [Tooltip("Strength of side-to-side random drift")]
    public float driftStrength = 0.5f;

    [Tooltip("Speed of drift changes")]
    public float driftFrequency = 1f;

    private Rigidbody rb;
    private Vector3 driftDirection;
    private float driftTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        driftDirection = Random.insideUnitSphere;
        driftDirection.y = 0f; // horizontal only
    }

    void FixedUpdate()
    {
        // 1. Buoyancy (upward force)
        Vector3 buoyancy = -Physics.gravity * rb.mass * buoyancyFactor;
        rb.AddForce(buoyancy, ForceMode.Force);

        // 2. Side drift
        driftTimer += Time.fixedDeltaTime;
        if (driftTimer >= driftFrequency)
        {
            driftTimer = 0f;
            driftDirection = Random.insideUnitSphere;
            driftDirection.y = 0f; // horizontal only
        }

        rb.AddForce(driftDirection * driftStrength, ForceMode.Force);

        // 3. Clamp vertical speed to prevent flying away
        Vector3 vel = rb.linearVelocity;
        vel.y = Mathf.Clamp(vel.y, -maxVerticalSpeed, maxVerticalSpeed);
        rb.linearVelocity = vel;
    }
}
