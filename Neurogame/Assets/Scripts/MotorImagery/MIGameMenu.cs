using NeuroFeedback;
using UnityEngine;

public class MIGameMenu : MonoBehaviour
{
  public GameObject StartGameButton;
  public GameObject PauseGameButton;
  public GameObject ResumeGameButton;

  public GameObject EndGameButton;
  public GameObject QuitGameButton;

  public GameObject GoToPhaseSelectButton;
  public GameObject GoToMainMenuButton;
  public GameObject[] PhaseSelectButtons;

  private bool gameRunning = false;

  void Start()
  {
    StartGameButton.SetActive(true);
    PauseGameButton.SetActive(false);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(true);
    GoToPhaseSelectButton.SetActive(true);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

  public void StartGame()
  {
    gameRunning = true;
    GameManager.Instance.StartGame();
    StartGameButton.SetActive(false);
    PauseGameButton.SetActive(true);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(false);
    GoToPhaseSelectButton.SetActive(false);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

  public void PauseGame()
  {
    GameManager.Instance.PauseGame();
    StartGameButton.SetActive(false);
    PauseGameButton.SetActive(false);
    ResumeGameButton.SetActive(true);
    EndGameButton.SetActive(true);
    QuitGameButton.SetActive(false);
    GoToPhaseSelectButton.SetActive(true);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

  public void ResumeGame()
  {
    gameRunning = true;
    GameManager.Instance.ResumeGame();
    StartGameButton.SetActive(false);
    PauseGameButton.SetActive(true);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(false);
    GoToPhaseSelectButton.SetActive(false);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

  public void EndGame()
  {
    gameRunning = false;
    GameManager.Instance.ResetGame();
    StartGameButton.SetActive(true);
    PauseGameButton.SetActive(false);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(true);
    GoToPhaseSelectButton.SetActive(true);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

  public void QuitGame()
  {
    gameRunning = false;
    StartGameButton.SetActive(true);
    PauseGameButton.SetActive(false);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(true);
    GoToPhaseSelectButton.SetActive(true);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }

    GameManager.Instance.QuitGame();
  }

  public void GoToPhaseSelect()
  {
    StartGameButton.SetActive(false);
    PauseGameButton.SetActive(false);
    ResumeGameButton.SetActive(false);
    EndGameButton.SetActive(false);
    QuitGameButton.SetActive(false);
    GoToPhaseSelectButton.SetActive(false);
    GoToMainMenuButton.SetActive(true);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(true);
    }
  }

  public void GoToMainMenu()
  {
    if (gameRunning)
    {
      StartGameButton.SetActive(false);
      ResumeGameButton.SetActive(true);
      EndGameButton.SetActive(true);
      QuitGameButton.SetActive(false);
    }
    else
    {
      StartGameButton.SetActive(true);
      ResumeGameButton.SetActive(false);
      EndGameButton.SetActive(false);
      QuitGameButton.SetActive(true);
    }

    PauseGameButton.SetActive(false);
    GoToPhaseSelectButton.SetActive(true);
    GoToMainMenuButton.SetActive(false);
    foreach (var button in PhaseSelectButtons)
    {
      button.SetActive(false);
    }
  }

}
