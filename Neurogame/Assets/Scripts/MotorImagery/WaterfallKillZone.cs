using UnityEngine;

public class WaterfallKillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        BallController ball = other.GetComponent<BallController>();
        if (ball != null)
        {
            GameManager.Instance.HandleBallDeath(ball);

        }
    }
}
