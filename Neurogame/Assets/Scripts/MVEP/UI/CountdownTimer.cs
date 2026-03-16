using UnityEngine;
using TMPro;
using System;

public class CountdownTimer : MonoBehaviour
{
  [SerializeField] private TMP_Text timerText;
  private float TimeRemaining = 3f;
  private bool TimerIsRunning = false;

  private readonly float rotateAmount = 30f;

  private Action onCompleteCallback;

  private Vector3 startingScale;

  private LTDescr currentAnimation;

  void Awake()
  {
    startingScale = transform.localScale;
  }

  void OnEnable()
  {
    MVEPGameEvents.OnGamePaused += HandleGamePaused;
    MVEPGameEvents.OnGameEnded += HandleGameEnded;
  }

  void OnDisable()
  {
    MVEPGameEvents.OnGamePaused -= HandleGamePaused;
    MVEPGameEvents.OnGameEnded -= HandleGameEnded;
  }

  public void StartCountdown(Action callback = null)
  {
    TimeRemaining = 3f;
    TimerIsRunning = true;

    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    gameObject.SetActive(true);
    AnimateScale();

    onCompleteCallback = callback;
  }

  void Update()
  {
    if (!TimerIsRunning) return;

    if (TimeRemaining > 0)
    {
      TimeRemaining -= Time.deltaTime;
      timerText.text = Mathf.Ceil(TimeRemaining).ToString();
    }
    else
    {
      TimeRemaining = 0;
      TimerIsRunning = false;
      timerText.text = "Go!";
      AnimateGo();
    }
  }

  void AnimateScale()
  {
    transform.localScale = startingScale * 0.8f;
    currentAnimation = LeanTween.scale(gameObject, startingScale * 1.2f, 1f).setRepeat(3).setEaseOutElastic();
  }

  void AnimateGo()
  {
    transform.Rotate(Vector3.forward, -rotateAmount);
    // rotate the timer on the z axis back and forth, like it's shaking with excitement, then fade out the text and disable the timer object
    currentAnimation = LeanTween.rotateZ(gameObject, rotateAmount, .2f).setRepeat(6).setLoopPingPong().setOnComplete(() =>
    {
      gameObject.SetActive(false);
      onCompleteCallback?.Invoke();
    });
  }

  private void HandleGamePaused()
  {
    if (!TimerIsRunning) return;
    TimerIsRunning = false;
    currentAnimation?.pause();
    gameObject.SetActive(false);
  }

  private void HandleGameEnded()
  {
    if (!TimerIsRunning) return;
    TimerIsRunning = false;
    currentAnimation?.pause();
    gameObject.SetActive(false);
  }
}
