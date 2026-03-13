using TMPro;
using UnityEngine;

public class MVEPInfoPanel : MonoBehaviour
{
  public GameObject startScreen;
  public GameObject endScreen;

  public TMP_Text scoreText;
  public TMP_Text powerUpText;
  public TMP_Text obstacleText;

  public void ShowStartScreen()
  {
    gameObject.SetActive(true);
    startScreen.SetActive(true);
    endScreen.SetActive(false);
  }

  public void ShowEndScreen(int totalScore, int powerUpsCollected, int obstaclesHit)
  {
    gameObject.SetActive(true);
    startScreen.SetActive(false);
    endScreen.SetActive(true);

    scoreText.text = $"{totalScore}";
    powerUpText.text = $"{powerUpsCollected}";
    obstacleText.text = $"{obstaclesHit}";
  }

  public void HideAll()
  {
    startScreen.SetActive(false);
    endScreen.SetActive(false);
    gameObject.SetActive(false);
  }
}