using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterfallFlowZone : MonoBehaviour
{
    [Header("Flow Settings")]

    [Tooltip("Direction the water flows in world space")]
    public Vector3 flowDirection = Vector3.back;

    [Tooltip("Speed of water flow")]
    public float flowSpeed = 4f;

    [Tooltip("Blend gravity into flow")]
    public bool applyGravity = false;

    private void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            ball.EnterWaterfall(flowDirection.normalized, flowSpeed, applyGravity);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            ball.ExitWaterfall();
        }
    }

}
