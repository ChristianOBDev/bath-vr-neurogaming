using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MotorImagery;
using TMPro;

public class PhaseMenuController : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty menuToggleAction;
    public InputActionProperty cycleAction;
    public InputActionProperty confirmAction;

    [Header("References")]
    public Transform leftController;
    public Canvas menuCanvas;

    [Header("Phase Buttons")]
    public GameObject phaseOneButton;
    public GameObject phaseTwoButton;
    public GameObject phaseThreeButton;

    [Header("Highlight Settings")]
    public bool useColorHighlight = false;
    public float normalScale = 1f;
    public float highlightScale = 1.2f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public float lerpSpeed = 8f;

    [Header("Positioning")]
    public Vector3 positionOffset = new Vector3(0f, 0.1f, 0f);
    public Vector3 rotationOffset = new Vector3(45f, 0f, 0f);

    private bool menuVisible = false;
    private int highlightedIndex = 0;
    private GameObject[] buttons;
    private Image[] buttonImages;
    private Vector3[] currentScales;
    private Color[] currentColors;

    void OnEnable()
    {
        menuToggleAction.action?.Enable();
        cycleAction.action?.Enable();
        confirmAction.action?.Enable();
    }

    void OnDisable()
    {
        menuToggleAction.action?.Disable();
        cycleAction.action?.Disable();
        confirmAction.action?.Disable();
    }

    void Start()
    {
        buttons = new GameObject[]
        {
            phaseOneButton,
            phaseTwoButton,
            phaseThreeButton
        };

        // Cache Image components and initialise scale/color arrays
        buttonImages = new Image[buttons.Length];
        currentScales = new Vector3[buttons.Length];
        currentColors = new Color[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            buttonImages[i] = buttons[i].GetComponent<Image>();
            currentScales[i] = Vector3.one * normalScale;
            currentColors[i] = normalColor;
        }

        menuCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (menuToggleAction.action != null && menuToggleAction.action.WasPressedThisFrame())
            ToggleMenu();

        if (menuVisible)
        {
            if (cycleAction.action != null && cycleAction.action.WasPressedThisFrame())
            {
                highlightedIndex = (highlightedIndex + 1) % buttons.Length;
            }

            if (confirmAction.action != null && confirmAction.action.WasPressedThisFrame())
            {
                ConfirmSelection();
            }

            if (leftController != null)
            {
                transform.position = leftController.position + positionOffset;
                transform.rotation = leftController.rotation * Quaternion.Euler(rotationOffset);
            }
        }

        // Always lerp scales and colors smoothly even when menu is hidden
        // so transitions are smooth when it reopens
        UpdateHighlightLerp();
    }

    void ToggleMenu()
    {
        menuVisible = !menuVisible;
        menuCanvas.gameObject.SetActive(menuVisible);

        if (menuVisible)
        {
            highlightedIndex = (int)PhaseManager.Instance.CurrentPhase;
        }
    }

    void UpdateHighlightLerp()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            bool isHighlighted = i == highlightedIndex;

            // Target values
            Vector3 targetScale = Vector3.one * (isHighlighted ? highlightScale : normalScale);
            Color targetColor = isHighlighted ? highlightColor : normalColor;

            // Lerp scale
            currentScales[i] = Vector3.Lerp(currentScales[i], targetScale, Time.deltaTime * lerpSpeed);
            buttons[i].transform.localScale = currentScales[i];

            // Lerp color
            if (buttonImages[i] != null && useColorHighlight)
            {
                currentColors[i] = Color.Lerp(currentColors[i], targetColor, Time.deltaTime * lerpSpeed);
                buttonImages[i].color = currentColors[i];
            }
        }
    }

    void ConfirmSelection()
    {
        GamePhase selectedPhase = (GamePhase)highlightedIndex;
        PhaseManager.Instance?.SetPhase(selectedPhase);
        GameManager.Instance?.ResetAndRespawn();
        Debug.Log($"Phase confirmed: {selectedPhase}");
        ToggleMenu();
    }
}