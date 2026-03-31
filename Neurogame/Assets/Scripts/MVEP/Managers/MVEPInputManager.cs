using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MVEPInputManager : MonoBehaviour
{
  // Lane input references
  [SerializeField] private InputActionReference lane1;
  [SerializeField] private InputActionReference lane2;
  [SerializeField] private InputActionReference lane3;
  [SerializeField] private InputActionReference lane4;
  [SerializeField] private InputActionReference lane5;

  public static Action<int> OnLaneInput;

  void OnEnable()
  {
    UDPManager.Instance.OnIntReceived += HandleInput;

    lane1.action.performed += (ctx) => OnLaneInput?.Invoke(0);
    lane2.action.performed += (ctx) => OnLaneInput?.Invoke(1);
    lane3.action.performed += (ctx) => OnLaneInput?.Invoke(2);
    lane4.action.performed += (ctx) => OnLaneInput?.Invoke(3);
    lane5.action.performed += (ctx) => OnLaneInput?.Invoke(4);
  }

  void OnDisable()
  {
    UDPManager.Instance.OnIntReceived -= HandleInput;

    lane1.action.performed -= (ctx) => OnLaneInput?.Invoke(0);
    lane2.action.performed -= (ctx) => OnLaneInput?.Invoke(1);
    lane3.action.performed -= (ctx) => OnLaneInput?.Invoke(2);
    lane4.action.performed -= (ctx) => OnLaneInput?.Invoke(3);
    lane5.action.performed -= (ctx) => OnLaneInput?.Invoke(4);
  }

  private void HandleInput(int laneIndex)
  {
    OnLaneInput?.Invoke(laneIndex - MVEPGameManager.Instance.inputOffset); // Convert to 0-based index
  }
}
