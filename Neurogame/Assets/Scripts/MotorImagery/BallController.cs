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
    [SerializeField] private BallState currentState;
    public BallState CurrentState => currentState;

    Rigidbody rb;

    [Header("Layers")]
    public string inactiveLayer = "Ball";
    public string activeLayer = "BallActive";

    [Header("Return Motion")]
    public float returnDuration = 7f;
    public AnimationCurve returnCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float bobAmplitude = 0.15f;
    public float bobFrequency = 2f;
    public bool enableBob = true;

    private Vector3 kickerTargetPos;

    [Header("Side Drift")]
    [Tooltip("Minimum sideways drift applied during return")]
    [SerializeField, Min(0f)]
    private float lateralDriftMin = 0.1f;

    [Tooltip("Maximum sideways drift applied during return")]
    [SerializeField, Min(0f)]
    private float lateralDriftMax = 0.7f;

    public bool enableLateralDrift = true;

    [Header("Clamp Velocity")]
    [SerializeField] private float maxVelocity = 20f;

    // Lateral offset randomly chosen when ball spawns
    private float lateralOffset = 0f;
    // Phase offset for independent side-to-side drift
    private float bobPhaseOffset = 0f;

    [Header("Waterfall Guidance")]
    public bool enableCenterGuidance = true;
    public Transform waterfallCenter;
    public float centerPullStrength = 2f;
    public float maxSideForce = 3f;
    [SerializeField] float centerDamping = 2f;

    Vector3 currentFlowDirection;
    float currentFlowSpeed;
    bool flowUsesGravity;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 upAxis = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Up
            : Vector3.up;

        // Apply flow forces if in waterfall or falling state
        if (currentState == BallState.OnWaterfall || currentState == BallState.Falling)
        {
            rb.AddForce(currentFlowDirection * currentFlowSpeed, ForceMode.Acceleration);
            return;
        }

        // LAUNCHING state guidance toward center of waterfall
        if (currentState == BallState.Launching && enableCenterGuidance)
        {
            if (waterfallCenter != null)
            {
                // Get the vector from ball to waterfall center
                Vector3 toCenter = waterfallCenter.position - transform.position;

                // Project onto the right axis to get purely sideways offset
                Vector3 rightAxis = SceneOrientation.Instance != null
                    ? SceneOrientation.Instance.Right
                    : Vector3.right;

                float offset = Vector3.Dot(toCenter, rightAxis);

                // Proportional pull toward center
                float proportionalForce = offset * centerPullStrength;

                // Damping based on current sideways velocity
                float dampingForce = -Vector3.Dot(rb.linearVelocity, rightAxis) * centerDamping;

                float totalForce = Mathf.Clamp(
                    proportionalForce + dampingForce,
                    -maxSideForce,
                    maxSideForce
                );

                rb.AddForce(rightAxis * totalForce, ForceMode.Force);

                Debug.DrawLine(
                    transform.position,
                    transform.position + rightAxis * totalForce,
                    Color.green
                );
            }

            if (rb.linearVelocity.magnitude > maxVelocity)
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
        
        // Push ball towards kicker in in IDLE state.
        if (currentState == BallState.IdleInPool)
        {
            if (rb.linearVelocity.magnitude < 0.1f)
            {
                Vector3 toKicker = (kickerTargetPos - transform.position).normalized;
                rb.AddForce(toKicker * 4.9f, ForceMode.Acceleration);
            }
        }
    }

    private void UpdateCollisionLayer()
    {
        bool bumperActive = currentState == BallState.OnWaterfall
                         || currentState == BallState.Falling;
        gameObject.layer = LayerMask.NameToLayer(bumperActive ? activeLayer : inactiveLayer);
    }

    IEnumerator ReturnToKicker(Vector3 targetPos)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        Vector3 upAxis = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Up
            : Vector3.up;

        Vector3 rightAxis = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Right
            : Vector3.right;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float curvedT = returnCurve.Evaluate(t);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, curvedT);

            if (enableLateralDrift)
            {
                float sideDrift = Mathf.Sin(Time.time * bobFrequency * 0.5f + bobPhaseOffset) * lateralOffset;
                pos += rightAxis * sideDrift;
            }

            if (enableBob)
            {
                float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                pos += upAxis * bob;
            }

            transform.position = pos;
            yield return null;
        }

        EnterIdleState();
    }

    void EnterIdleState()
    {
        rb.isKinematic = false;
        SetState(BallState.IdleInPool);
    }

    public void SetState(BallState newState)
    {
        if (GameManager.Instance != null && GameManager.Instance.verboseLogging)
            Debug.Log("Ball State changed to: " + newState);
        currentState = newState;
        UpdateCollisionLayer();
    }

    public void BeginReturnPhase(Vector3 targetPos)
    {
        kickerTargetPos = targetPos;

        SetState(BallState.ReturningToKicker);

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
        SetState(BallState.OnWaterfall);

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
            SetState(BallState.Falling); // free physics takes over

            rb.useGravity = true;
            rb.isKinematic = false; // make sure physics fully active.
        }
    }

}
