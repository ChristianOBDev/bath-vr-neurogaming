using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(TrackedDeviceGraphicRaycaster))]
public class WorldSpaceCanvas : MonoBehaviour
{
  private Canvas canvas;

  void Awake()
  {
    if (canvas == null)
      canvas = GetComponent<Canvas>();
    SetCamera();
  }

  void OnEnable()
  {
    SetCamera();
  }

  void Start()
  {
    SetCamera();
  }

  void SetCamera()
  {
    if (canvas == null)
      canvas = GetComponent<Canvas>();
    if (Camera.main != null && canvas.worldCamera != Camera.main)
      canvas.worldCamera = Camera.main;
  }
}
