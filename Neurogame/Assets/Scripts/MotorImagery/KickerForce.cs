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

    private Rigidbody currentBalloon;

    void OnCollisionEnter(Collision collision)
    {
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

        // Base upward direction
        Vector3 direction = Vector3.up;

        // Lateral influence based on contact position
        Vector3 contactOffset =
            currentBalloon.position - transform.position;

        Vector3 lateral =
            Vector3.ProjectOnPlane(contactOffset, Vector3.up).normalized;

        direction += lateral * lateralInfluence;

        // Enforce minimum launch angle
        float minY = Mathf.Tan(minLaunchAngle * Mathf.Deg2Rad);
        direction.y = Mathf.Max(direction.y, minY);

        direction.Normalize();

        currentBalloon.AddForce(
            direction * launchForce * strength,
            ForceMode.Force
        );

        // Clamp upward velocity

        if (strength > 0f)
        {
            Vector3 v = currentBalloon.linearVelocity;
            if (v.y > maxUpwardVelocity)
            {
                v.y = maxUpwardVelocity;
                currentBalloon.linearVelocity = v;
            }
        }

    }
}
