using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MotorImagery;

public class PhaseMenuController : MonoBehaviour
{
  [Header("Phase Buttons")]
  public GameObject phaseOneButton;
  public GameObject phaseTwoButton;
  public GameObject phaseThreeButton;

  [Header("Highlight Settings")]
  public bool useColorHighlight = false;
  public float normalScale = 1f;
  public float highlightScale = 1.2f;
  public float transitionDuration = 0.25f;
  public Color normalColor = Color.white;
  public Color highlightColor = Color.yellow;

  private GameObject[] buttons;
  private Image[] buttonImages;

  void Start()
  {
    buttons = new GameObject[]
    {
            phaseOneButton,
            phaseTwoButton,
            phaseThreeButton
    };

    buttonImages = new Image[]
    {
      phaseOneButton.GetComponent<Image>(),
      phaseTwoButton.GetComponent<Image>(),
      phaseThreeButton.GetComponent<Image>()
    };
  }

  public void SetPhase(int phaseIndex)
  {
    if (phaseIndex < 0 || phaseIndex > (int)GamePhase.PhaseThree) return;
    if (PhaseManager.Instance.CurrentPhase == (GamePhase)phaseIndex) return;
    PhaseManager.Instance.SetPhase((GamePhase)phaseIndex);
    // GameManager.Instance.ResetAndRespawn();
    LerpButtonVisuals(phaseIndex);
  }

  private void LerpButtonVisuals(int selectedIndex)
  {
    for (int i = 0; i < buttons.Length; i++)
    {
      if (buttons[i] == null) continue;

      bool isHighlighted = i == selectedIndex;

      // Target values
      Vector3 targetScale = Vector3.one * (isHighlighted ? highlightScale : normalScale);
      Color targetColor = isHighlighted ? highlightColor : normalColor;

      buttons[i].LeanScale(targetScale, transitionDuration).setEaseInSine().setEaseOutBounce();
      if (useColorHighlight && buttonImages[i] != null)
        LeanTween.color(buttonImages[i].rectTransform, targetColor, transitionDuration).setEaseInOutSine();
    }
  }
}