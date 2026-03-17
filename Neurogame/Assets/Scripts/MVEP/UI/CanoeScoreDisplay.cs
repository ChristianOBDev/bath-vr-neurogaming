using UnityEngine;
using TMPro;

/// <summary>
/// Displays and animates the player's score on the UI.
/// Handles score updates with smooth animations, bonus text, and penalty text.
/// Automatically updates visuals in response to game events.
/// </summary>
public class CanoeScoreDisplay : MonoBehaviour
{
  // Constants - Animations
  private const float BONUS_MOVE_DISTANCE = 300f;
  private const float BONUS_ANIMATION_DURATION = 1.25f;
  private const float PENALTY_MOVE_DISTANCE = 300f;
  private const float PENALTY_ANIMATION_DURATION = 0.5f;
  private const float MIN_SCORE_ANIMATION_DURATION = 0.2f;
  private const float MAX_SCORE_ANIMATION_DURATION = 1.0f;
  private const float SCORE_ANIMATION_DURATION_MULTIPLIER = 0.05f;

  // Configuration - References
  [SerializeField] private TMP_Text scoreText;
  [SerializeField] private TMP_Text bonusText;
  [SerializeField] private TMP_Text penaltyText;

  // State - Score Display
  private int displayedScore = 0;
  private LTDescr scoreTween;

  /// <summary>
  /// Subscribes to game events that affect the score display.
  /// </summary>
  private void OnEnable()
  {
    MVEPGameEvents.OnPowerUpCollected += HandlePowerUpCollected;
    MVEPGameEvents.OnObstacleHit += HandleObstacleHit;
    MVEPGameEvents.OnScoreUpdated += HandleScoreUpdated;
  }

  /// <summary>
  /// Unsubscribes from game events.
  /// </summary>
  private void OnDisable()
  {
    MVEPGameEvents.OnPowerUpCollected -= HandlePowerUpCollected;
    MVEPGameEvents.OnObstacleHit -= HandleObstacleHit;
    MVEPGameEvents.OnScoreUpdated -= HandleScoreUpdated;
  }

  /// <summary>
  /// Handles score update events with smooth animated transitions.
  /// Cancels any existing animation and plays a new one from current to target score.
  /// </summary>
  /// <param name="score">The new score to display.</param>
  private void HandleScoreUpdated(int score)
  {
    CancelScoreTween();
    AnimateScoreTransition(displayedScore, score);
  }

  /// <summary>
  /// Animates the score text from start to end value over a calculated duration.
  /// Duration scales with the distance traveled (0.05 seconds per point).
  /// </summary>
  /// <param name="startScore">Starting score value.</param>
  /// <param name="endScore">Target score value.</param>
  private void AnimateScoreTransition(int startScore, int endScore)
  {
    float duration = CalculateScoreAnimationDuration(startScore, endScore);

    scoreTween = LeanTween.value(gameObject, startScore, endScore, duration)
      .setOnUpdate(val =>
      {
        displayedScore = Mathf.RoundToInt(val);
        UpdateScoreTextDisplay(displayedScore);
      })
      .setOnComplete(() => FinalizeScoreAnimation(endScore));
  }

  /// <summary>
  /// Calculates the appropriate animation duration based on score difference.
  /// Larger score changes get longer animations for better visibility.
  /// </summary>
  /// <param name="startScore">Starting score.</param>
  /// <param name="endScore">Target score.</param>
  /// <returns>Calculated duration in seconds.</returns>
  private float CalculateScoreAnimationDuration(int startScore, int endScore)
  {
    float baseDuration = Mathf.Abs(endScore - startScore) * SCORE_ANIMATION_DURATION_MULTIPLIER;
    return Mathf.Clamp(baseDuration, MIN_SCORE_ANIMATION_DURATION, MAX_SCORE_ANIMATION_DURATION);
  }

  /// <summary>
  /// Updates the score text display with the given value.
  /// </summary>
  /// <param name="score">Score value to display.</param>
  private void UpdateScoreTextDisplay(int score)
  {
    if (scoreText != null)
    {
      scoreText.text = score.ToString();
    }
  }

  /// <summary>
  /// Finalizes the score animation by ensuring the final value is displayed.
  /// </summary>
  /// <param name="finalScore">The final score value.</param>
  private void FinalizeScoreAnimation(int finalScore)
  {
    displayedScore = finalScore;
    UpdateScoreTextDisplay(finalScore);
    scoreTween = null;
  }

  /// <summary>
  /// Cancels any ongoing score animation.
  /// </summary>
  private void CancelScoreTween()
  {
    if (scoreTween != null && LeanTween.isTweening(scoreTween.uniqueId))
    {
      LeanTween.cancel(scoreTween.uniqueId);
    }
  }

  /// <summary>
  /// Handles power-up collection by animating the bonus text.
  /// </summary>
  private void HandlePowerUpCollected()
  {
    AnimateFloatingText(bonusText, BONUS_MOVE_DISTANCE, BONUS_ANIMATION_DURATION);
  }

  /// <summary>
  /// Handles obstacle collision by animating the penalty text.
  /// </summary>
  private void HandleObstacleHit()
  {
    AnimateFloatingText(penaltyText, PENALTY_MOVE_DISTANCE, PENALTY_ANIMATION_DURATION);
  }

  /// <summary>
  /// Animates a floating text element moving upward and then resetting.
  /// </summary>
  /// <param name="textElement">The text element to animate.</param>
  /// <param name="moveDistance">Distance to move upward in pixels.</param>
  /// <param name="duration">Duration of the animation in seconds.</param>
  private void AnimateFloatingText(TMP_Text textElement, float moveDistance, float duration)
  {
    if (textElement == null)
      return;

    textElement.gameObject.SetActive(true);
    Vector3 originalPosition = textElement.transform.localPosition;
    Vector3 targetPosition = originalPosition + Vector3.up * moveDistance;

    textElement.transform.LeanMoveLocal(targetPosition, duration)
      .setEaseOutSine()
      .setOnComplete(() => ResetFloatingText(textElement, originalPosition));
  }

  /// <summary>
  /// Resets a floating text element to its original position and hides it.
  /// </summary>
  /// <param name="textElement">The text element to reset.</param>
  /// <param name="originalPosition">The original position to return to.</param>
  private void ResetFloatingText(TMP_Text textElement, Vector3 originalPosition)
  {
    if (textElement != null)
    {
      textElement.transform.localPosition = originalPosition;
      textElement.gameObject.SetActive(false);
    }
  }
}
