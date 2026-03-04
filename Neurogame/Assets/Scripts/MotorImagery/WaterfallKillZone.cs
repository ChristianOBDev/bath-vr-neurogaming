using UnityEngine;

public class WaterfallKillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball == null) return;
        if (ball.CurrentState == BallState.ReturningToKicker) return;

        GameManager.Instance.HandleBallDeath(ball);
    }
}