using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Audio.GeneratorInstance;

public class PhaseMenuController : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty menuToggleAction;

    [Header("References")]
    public Transform leftController;
    public Canvas menuCanvas;

    [Header("Positioning")]
    public Vector3 positionOffset = new Vector3(0f, 0.1f, 0f);
    public Vector3 rotationOffset = new Vector3(45f, 0f, 0f);

    [Header("Buttons")]
    public Button phaseOneButton;
    public Button phaseTwoButton;
    public Button phaseThreeButton;

    private bool menuVisible = false;

    void OnEnable()
    {
        if (menuToggleAction.action != null)
        {
            menuToggleAction.action.Enable();
            Debug.Log("Menu toggle action enabled.");
        }
        else
        {
            Debug.LogWarning("menuToggleAction.action is null in OnEnable!");
        }
    }

    void OnDisable()
    {
        if (menuToggleAction.action != null)
            menuToggleAction.action.Disable();
    }

    void Start()
    {
        // Hide menu at start
        menuCanvas.gameObject.SetActive(false);

        // Wire up buttons
        if (phaseOneButton != null)
            phaseOneButton.onClick.AddListener(() => SelectPhase(GamePhase.PhaseOne));
        if (phaseTwoButton != null)
            phaseTwoButton.onClick.AddListener(() => SelectPhase(GamePhase.PhaseTwo));
        if (phaseThreeButton != null)
            phaseThreeButton.onClick.AddListener(() => SelectPhase(GamePhase.PhaseThree));
    }

    void Update()
    {
        if (menuToggleAction.action != null && menuToggleAction.action.WasPressedThisFrame())
        {
            Debug.Log($"Action enabled: {menuToggleAction.action.enabled}, triggered: {menuToggleAction.action.triggered}");
            ToggleMenu();
        }
        else if (menuToggleAction.action == null)
        {
            Debug.LogWarning("menuToggleAction.action is null!");
        }

        if (menuVisible && leftController != null)
        {
            transform.position = leftController.position + positionOffset;
            transform.rotation = leftController.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    void ToggleMenu()
    {
        menuVisible = !menuVisible;
        menuCanvas.gameObject.SetActive(menuVisible);
    }

    void SelectPhase(GamePhase phase)
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.SetPhase(phase);

        // Optionally auto-dismiss after selection
        ToggleMenu();
    }
}
