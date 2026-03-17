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

  // Test inputs
  [SerializeField] private InputActionReference startGame;
  [SerializeField] private InputActionReference pauseGame;
  [SerializeField] private InputActionReference resumeGame;
  [SerializeField] private InputActionReference quitGame;

  [SerializeField] private InputActionAsset inputActions;

  void OnEnable()
  {
    inputActions.Enable();

    lane1.action.performed += (ctx) => OnLaneInput?.Invoke(0);
    lane2.action.performed += (ctx) => OnLaneInput?.Invoke(1);
    lane3.action.performed += (ctx) => OnLaneInput?.Invoke(2);
    lane4.action.performed += (ctx) => OnLaneInput?.Invoke(3);
    lane5.action.performed += (ctx) => OnLaneInput?.Invoke(4);

    startGame.action.performed += StartGame;
    pauseGame.action.performed += PauseGame;
    resumeGame.action.performed += ResumeGame;
    quitGame.action.performed += QuitGame;
  }

  void OnDisable()
  {
    inputActions.Disable();

    lane1.action.performed -= (ctx) => OnLaneInput?.Invoke(0);
    lane2.action.performed -= (ctx) => OnLaneInput?.Invoke(1);
    lane3.action.performed -= (ctx) => OnLaneInput?.Invoke(2);
    lane4.action.performed -= (ctx) => OnLaneInput?.Invoke(3);
    lane5.action.performed -= (ctx) => OnLaneInput?.Invoke(4);

    startGame.action.performed -= StartGame;
    pauseGame.action.performed -= PauseGame;
    resumeGame.action.performed -= ResumeGame;
    quitGame.action.performed -= QuitGame;
  }

  void StartGame(InputAction.CallbackContext context)
  {
    MVEPGameManager.Instance.StartGame();
  }

  void PauseGame(InputAction.CallbackContext context)
  {
    MVEPGameManager.Instance.PauseGame();
  }

  void ResumeGame(InputAction.CallbackContext context)
  {
    MVEPGameManager.Instance.ResumeGame();
  }

  void QuitGame(InputAction.CallbackContext context)
  {
    MVEPGameManager.Instance.QuitGame();
  }

}
