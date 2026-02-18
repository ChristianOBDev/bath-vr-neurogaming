using System.Collections;
using UnityEngine;

public enum BallState
{
    IdleInPool,
    Launching,
    OnWaterfall,
    ReturningToKicker
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



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

}
