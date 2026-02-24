using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MVEPInputManager : MonoBehaviour
{
  [SerializeField] private InputActionReference lane1;
  [SerializeField] private InputActionReference lane2;
  [SerializeField] private InputActionReference lane3;
  [SerializeField] private InputActionReference lane4;
  [SerializeField] private InputActionReference lane5;

  public static Action<int> OnLaneInput;

  void OnEnable()
  {
    lane1.action.performed += (ctx) => OnLaneInput?.Invoke(0);
    lane2.action.performed += (ctx) => OnLaneInput?.Invoke(1);
    lane3.action.performed += (ctx) => OnLaneInput?.Invoke(2);
    lane4.action.performed += (ctx) => OnLaneInput?.Invoke(3);
    lane5.action.performed += (ctx) => OnLaneInput?.Invoke(4);
  }
}
