using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KickerForce : MonoBehaviour
{
    [Header("References")]
    public KickerInputRouter inputRouter;

    [Tooltip("True = Left (Blue), False = Right (Red)")]
    public bool isLeftKicker;

    [Header("Force Settings")]
    [Tooltip("Total launch force applied by the kicker")]
    public float launchForce = 15f;

    [Tooltip("How much lateral influence contact position has")]
    [Range(0f, 1f)]
    public float lateralInfluence = 0.25f;

    [Tooltip("Minimum launch angle in degrees above horizontal")]
    [Range(45f, 90f)]
    public float minLaunchAngle = 70f;

    [Header("Velocity Control")]
    [Tooltip("Maximum upward speed allowed from kickers")]
    public float maxUpwardVelocity = 4.5f;

    [Tooltip("If false, force is always max when active")]
    public bool graduatedForce = true;

    [Header("Launch Direction")]
    [Tooltip("Unit vector pointing from kicker toward the waterfall")]
    public Vector3 forwardDirection = Vector3.forward;

    private Rigidbody currentBalloon;

    void OnCollisionEnter(Collision collision)
    {
        BallController ballController = collision.gameObject.GetComponent<BallController>();

        if (ballController != null)
        {
            ballController.SetState(BallState.Launching);
        }

        if (collision.rigidbody != null)
        {
            currentBalloon = collision.rigidbody;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == currentBalloon)
        {
            currentBalloon = null;
        }
    }

    void FixedUpdate()
    {
        if (currentBalloon == null || inputRouter == null)
            return;

        float strength = inputRouter.GetStrength(isLeftKicker, graduatedForce);
        if (strength <= 0f)
            return;

        // --- Build the launch direction ---
        Vector3 direction = forwardDirection.normalized;

        // Add upward component for minimum launch angle
        float upY = Mathf.Tan(minLaunchAngle * Mathf.Deg2Rad);
        direction.y += upY;

        // Add lateral influence based on contact position
        Vector3 contactOffset = currentBalloon.position - transform.position;
        Vector3 lateral = Vector3.ProjectOnPlane(contactOffset, Vector3.up).normalized;
        direction += lateral * lateralInfluence;

        // Normalize to keep force consistent
        direction.Normalize();

        // Apply force to the ball
        currentBalloon.AddForce(direction * launchForce * strength, ForceMode.Force);

        // Clamp upward velocity
        Vector3 v = currentBalloon.linearVelocity;
        if (v.y > maxUpwardVelocity)
        {
            v.y = maxUpwardVelocity;
            currentBalloon.linearVelocity = v;  
        }
    }
}
