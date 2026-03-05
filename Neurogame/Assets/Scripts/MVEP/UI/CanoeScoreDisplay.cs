using UnityEngine;
using TMPro;

public class CanoeScoreDisplay : MonoBehaviour
{
  [SerializeField] private TMP_Text scoreText;
  private int displayedScore = 0;
  private LTDescr scoreTween;

  void OnEnable()
  {
    // GameEvents.OnPowerUpCollected += AnimateBonus;
    // GameEvents.OnObstacleHit += AnimatePenalty;
    MVEPGameEvents.OnScoreUpdated += UpdateScoreDisplay;
  }

  void OnDisable()
  {
    // GameEvents.OnPowerUpCollected -= AnimateBonus;
    // GameEvents.OnObstacleHit -= AnimatePenalty;
    MVEPGameEvents.OnScoreUpdated -= UpdateScoreDisplay;
  }

  private void UpdateScoreDisplay(int score)

  {
    // If an animation is running, cancel it
    if (scoreTween != null && LeanTween.isTweening(scoreTween.uniqueId))
    {
      LeanTween.cancel(scoreTween.uniqueId);
    }

    int startScore = displayedScore;
    int endScore = score;
    float duration = Mathf.Clamp(Mathf.Abs(endScore - startScore) * 0.05f, 0.2f, 1.0f); // Adjust duration as needed

    scoreTween = LeanTween.value(gameObject, startScore, endScore, duration)
      .setOnUpdate(val =>
      {
        displayedScore = Mathf.RoundToInt(val);
        scoreText.text = displayedScore.ToString();
      })
      .setOnComplete(() =>
      {
        displayedScore = endScore;
        scoreText.text = endScore.ToString();
        scoreTween = null;
      });
  }
}
