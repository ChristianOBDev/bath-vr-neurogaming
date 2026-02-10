using UnityEngine;

public class MVEPStimulus : MonoBehaviour
{
  [Header("Component References")]
  [SerializeField] private RectTransform line;

  [Header("Rect Settings")]
  [SerializeField] private Vector2 startPos = new(45f, 0f), endPos = new(-45f, 0f);
  [SerializeField] private float pulseDuration = 0.14f;

  public void Pulse()
  {
    line.anchoredPosition = startPos;
    line.gameObject.SetActive(true);
    line.LeanMoveLocalX(endPos.x, pulseDuration).setOnComplete(EndPulse);
  }

  private void EndPulse()
  {
    line.gameObject.SetActive(false);
    line.anchoredPosition = startPos;
  }
}
