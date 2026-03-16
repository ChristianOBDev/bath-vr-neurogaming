using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AngularCanvasSizer : MonoBehaviour
{
  [SerializeField] private Camera targetCamera;

  [SerializeField] private float horizontalDegrees;

  private RectTransform rectTransform;
  private float baseWidth;

  private void Awake()
  {
    rectTransform = GetComponent<RectTransform>();

    if (targetCamera == null)
      targetCamera = Camera.main;

    baseWidth = rectTransform.rect.width;
  }

  void LateUpdate()
  {
    if (targetCamera == null)
      return;

    ApplySizing();
  }

  private void ApplySizing()
  {
    float distance = Vector3.Distance(
        rectTransform.position,
        targetCamera.transform.position
    );

    float angleRad = horizontalDegrees * Mathf.Deg2Rad;
    float width = 2f * distance * Mathf.Tan(angleRad * 0.5f);

    float scale = width / baseWidth;
    transform.localScale = Vector3.one * scale;
  }
}
