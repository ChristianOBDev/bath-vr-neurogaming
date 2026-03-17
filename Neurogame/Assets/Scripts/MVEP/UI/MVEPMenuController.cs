using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MVEPMenuController : MonoBehaviour
{
  [SerializeField] private GameObject startButton;
  [SerializeField] private GameObject pauseButton;
  [SerializeField] private GameObject resumeButton;
  [SerializeField] private GameObject endButton;
  [SerializeField] private GameObject quitButton;
  [SerializeField] private GameObject phaseSelectButton;
  [SerializeField] private GameObject phaseSelectTitle;
  [SerializeField] private GameObject[] phaseButtons;
  [SerializeField] private Image[] phaseButtonImages;

  void OnEnable()
  {
    MVEPGameEvents.OnGameStarted += HandleGameStarted;
    MVEPGameEvents.OnGamePaused += HandleGamePaused;
    MVEPGameEvents.OnGameResumed += HandleGameResumed;
    MVEPGameEvents.OnGameEnded += HandleGameEnded;
    MVEPGameEvents.OnGameQuit += HandleGameQuit;

    MVEPGameEvents.OnPhaseChanged += HandlePhaseChanged;
  }

  void OnDisable()
  {
    MVEPGameEvents.OnGameStarted -= HandleGameStarted;
    MVEPGameEvents.OnGamePaused -= HandleGamePaused;
    MVEPGameEvents.OnGameResumed -= HandleGameResumed;
    MVEPGameEvents.OnGameEnded -= HandleGameEnded;
    MVEPGameEvents.OnGameQuit -= HandleGameQuit;

    MVEPGameEvents.OnPhaseChanged -= HandlePhaseChanged;
  }

  private void Start()
  {
    ActivateMenu();
  }

  private void HandleGameNotStarted()
  {
    gameObject.SetActive(true);
    startButton.SetActive(true);
    pauseButton.SetActive(false);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    quitButton.SetActive(true);
    phaseSelectButton.SetActive(true);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  public void HandleGameStarted()
  {
    gameObject.SetActive(true);
    startButton.SetActive(false);
    pauseButton.SetActive(true);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    quitButton.SetActive(false);
    phaseSelectButton.SetActive(false);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  private void HandleGamePaused()
  {
    gameObject.SetActive(true);
    startButton.SetActive(false);
    pauseButton.SetActive(false);
    resumeButton.SetActive(true);
    endButton.SetActive(true);
    quitButton.SetActive(false);
    phaseSelectButton.SetActive(true);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  private void HandleGameResumed()
  {
    gameObject.SetActive(true);
    startButton.SetActive(false);
    pauseButton.SetActive(true);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    quitButton.SetActive(false);
    phaseSelectButton.SetActive(false);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  private void HandleGameEnded()
  {
    gameObject.SetActive(true);
    startButton.SetActive(true);
    pauseButton.SetActive(false);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    quitButton.SetActive(true);
    phaseSelectButton.SetActive(true);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  private void HandleGameQuit()
  {
    gameObject.SetActive(false);
    startButton.SetActive(false);
    pauseButton.SetActive(false);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    quitButton.SetActive(false);
    phaseSelectButton.SetActive(false);
    phaseSelectTitle.SetActive(false);

    foreach (var button in phaseButtons)
    {
      button.SetActive(false);
    }
  }

  private void HandlePhaseChanged(MVEPGamePhase newPhase)
  {
    foreach (var image in phaseButtonImages)
    {
      image.color = Color.white; // Reset all to default color
    }

    int phaseIndex = (int)newPhase;
    if (phaseIndex >= 0 && phaseIndex < phaseButtonImages.Length)
    {
      phaseButtonImages[phaseIndex].color = Color.green; // Highlight current phase
    }
  }

  public void GoToPhaseSelect()
  {
    gameObject.SetActive(true);
    startButton.SetActive(false);
    pauseButton.SetActive(false);
    resumeButton.SetActive(false);
    endButton.SetActive(false);
    phaseSelectButton.SetActive(false);
    phaseSelectTitle.SetActive(true);

    foreach (var button in phaseButtons)
    {
      button.SetActive(true);
    }
  }

  public void ActivateMenu()
  {
    switch (MVEPGameManager.Instance.CurrentState)
    {
      case MVEPGameState.NotStarted:
        HandleGameNotStarted();
        break;
      case MVEPGameState.Running:
        HandleGameStarted();
        break;
      case MVEPGameState.Paused:
        HandleGamePaused();
        break;
      case MVEPGameState.Ended:
        HandleGameNotStarted();
        break;
      case MVEPGameState.Quit:
        HandleGameQuit();
        break;
    }
  }
}
