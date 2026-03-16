using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterfallFlowZone : MonoBehaviour
{
    [Header("Flow Settings")]
    [Tooltip("Direction the water flows in LOCAL space")]
    public Vector3 flowDirection = Vector3.back;
    [Tooltip("Speed of water flow")]
    public float flowSpeed = 4f;
    [Tooltip("Blend gravity into flow")]
    public bool applyGravity = false;

    private Vector3 WorldFlowDirection =>
        SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Resolve(flowDirection.normalized)
            : flowDirection.normalized;

    private void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
            ball.EnterWaterfall(WorldFlowDirection, flowSpeed, applyGravity);
    }

    private void OnTriggerStay(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null && ball.CurrentState != BallState.OnWaterfall)
            ball.EnterWaterfall(WorldFlowDirection, flowSpeed, applyGravity);
    }

    private void OnTriggerExit(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
            ball.ExitWaterfall();
    }
}