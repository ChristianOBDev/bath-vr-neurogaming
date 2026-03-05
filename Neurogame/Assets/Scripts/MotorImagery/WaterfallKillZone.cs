using UnityEngine;

public class WaterfallKillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball == null) return;
        if (ball.CurrentState == BallState.ReturningToKicker) return;
        //if (ball.CurrentState == BallState.IdleInPool) return;

        if (GameManager.Instance != null)
            GameManager.Instance.HandleBallDeath(ball);
    }
}