using System.Collections;
using UnityEngine;

public enum BallState
{
    IdleInPool,
    Launching,
    OnWaterfall,
    ReturningToKicker,
    Falling // NEW: physics-driven free-fall after leaving flowzones
}

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public BallState currentState;

    Rigidbody rb;

    [Header("Return Motion")]
    public float returnDuration = 7f;
    public AnimationCurve returnCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float bobAmplitude = 0.15f;
    public float bobFrequency = 2f;

    [Header("Side Drift")]
    [Tooltip("Minimum sideways drift applied during return")]
    [SerializeField, Min(0f)]
    private float lateralDriftMin = 0.1f;

    [Tooltip("Maximum sideways drift applied during return")]
    [SerializeField, Min(0f)]
    private float lateralDriftMax = 0.7f;

    // Lateral offset randomly chosen when ball spawns
    private float lateralOffset = 0f;
    // Phase offset for independent side-to-side drift
    private float bobPhaseOffset = 0f;

    [Header("Waterfall Guidance")]

    [Tooltip("Enable gentle horizontal correction toward waterfall center")]
    public bool enableCenterGuidance = true;

    [Tooltip("World-space X position of waterfall center")]
    public float waterfallCenterX = 0f;

    [Tooltip("Half width of the waterfall (for tuning reference only)")]
    public float waterfallHalfWidth = 4f;

    [Tooltip("How strongly the ball is pulled toward center")]
    public float centerPullStrength = 2f;

    [Tooltip("Maximum sideways force that can be applied")]
    public float maxSideForce = 3f;

    Vector3 currentFlowDirection;
    float currentFlowSpeed;
    bool flowUsesGravity;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // ------------------- WATERFALL STATE -------------------
        // Ball is on the top/face of the waterfall and is being carried by the flow
        if (currentState == BallState.OnWaterfall)
        {
            Vector3 flowVelocity = currentFlowDirection * currentFlowSpeed;

            if (flowUsesGravity)
                flowVelocity += Physics.gravity;

            // Apply flow velocity directly; overrides other forces
            rb.linearVelocity = flowVelocity;

            return; // Skip center guidance or other logic while flowing
        }

        // ------------------- LAUNCH STATE (CENTER GUIDANCE) -------------------
        // Pull ball toward waterfall center line while it is being launched
        if (currentState == BallState.Launching && enableCenterGuidance)
        {
            float offset = waterfallCenterX - transform.position.x;

            // Proportional pull toward centerline
            float force = offset * centerPullStrength;

            // Clamp to avoid extreme sideways force
            force = Mathf.Clamp(force, -maxSideForce, maxSideForce);

            rb.AddForce(Vector3.right * force, ForceMode.Force);
        }

        // ------------------- FALLING STATE -------------------
        // Ball has left the waterfall, now physics takes over naturally
        if (currentState == BallState.Falling)
        {
            // No special movement; physics (gravity, collisions) drives the ball
            // Optional: can clamp velocity if desired
            // Example: rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxFallSpeed);
        }
    }




    IEnumerator ReturnToKicker(Vector3 targetPos)
    {
        float elapsed = 0f;

        Vector3 startPos = transform.position;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;

            float curvedT = returnCurve.Evaluate(t);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, curvedT);

            // Add side to side drift with independent phase
            float sideDrift = Mathf.Sin(Time.time * bobFrequency * 0.5f + bobPhaseOffset) * lateralOffset;
            pos += Vector3.right * sideDrift;


            // Add bobbing motion
            float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            pos += Vector3.up * bob;

            transform.position = pos;

            yield return null;
        }

        EnterIdleState();
    }

    void EnterIdleState()
    {
        rb.isKinematic = false;
        currentState = BallState.IdleInPool;
    }

    public void BeginReturnPhase(Vector3 targetPos)
    {
        currentState = BallState.ReturningToKicker;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Determine lateral drift
        float magnitude = Random.Range(lateralDriftMin, lateralDriftMax);
        float direction = Random.value < 0.5f ? -1f : 1f;
        lateralOffset = magnitude * direction;

        // Assign random phase for side drift
        bobPhaseOffset = Random.Range(0f, Mathf.PI * 2f);

        StartCoroutine(ReturnToKicker(targetPos));
    }

    public void EnterWaterfall(Vector3 direction, float speed, bool useGravity)
    {
        currentState = BallState.OnWaterfall;

        currentFlowDirection = direction;
        currentFlowSpeed = speed;
        flowUsesGravity = useGravity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    public void ExitWaterfall()
    {
        if (currentState == BallState.OnWaterfall)
        {
            currentState = BallState.Falling; // free physics takes over

            rb.useGravity = true;
            rb.isKinematic = false; // make sure physics fully active.
        }
    }

}
