using UnityEngine;

public class BlockerController : MonoBehaviour
{
    private BallController ball;
    private Collider blockerCollider;

    void Awake()
    {
        blockerCollider = GetComponent<Collider>();
        if (blockerCollider != null)
            blockerCollider.enabled = false; // start disabled
    }

    void Update()
    {
        if (ball == null)
        {
            ball = FindFirstObjectByType<BallController>();
            if (ball == null)
                return;
        }

        bool shouldBeActive =
            ball.CurrentState == BallState.OnWaterfall ||
            ball.CurrentState == BallState.Falling;

        if (blockerCollider != null)
            blockerCollider.enabled = shouldBeActive;
    }
}