using UnityEngine;
using System;

public enum GamePhase
{
  PhaseOne,
  PhaseTwo,
  PhaseThree
}
namespace MotorImagery
{
  public class PhaseManager : MonoBehaviour
  {
    public static PhaseManager Instance { get; private set; }

    [Header("Debug UI")]
    public bool showDebugUI = true;

    [Header("Current Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.PhaseOne;
    public GamePhase CurrentPhase => currentPhase;

    [Header("Kicker References")]
    public KickerForce leftKicker;
    public KickerForce rightKicker;
    public KickerInputRouter inputRouter;

    [Header("Phase Two Settings")]
    [Range(0f, 1f)]
    public float phaseTwoMinStrength = 0.5f;

    [Header("Automation (optional)")]
    public bool autoAdvanceFromPhaseOne = false;
    public bool autoAdvanceFromPhaseTwo = false;
    public int phaseTwoBallThreshold = 0;
    public int phaseThreeBallThreshold = 0;
    public int phaseTwoScoreThreshold = 0;
    public int phaseThreeScoreThreshold = 0;

    public event Action<GamePhase> OnPhaseChanged;

    void Awake()
    {
      if (Instance != null && Instance != this) { Destroy(gameObject); return; }
      Instance = this;
    }

    void Start()
    {
      ApplyPhase(currentPhase);
    }

    void Update()
    {
      HandleAutoAdvance();
    }

    public void SetPhase(GamePhase newPhase)
    {
      if (newPhase == currentPhase) return;
      currentPhase = newPhase;
      ApplyPhase(currentPhase);
      OnPhaseChanged?.Invoke(currentPhase);
      Debug.Log($"Phase changed to: {currentPhase}");
    }

    public void AdvancePhase()
    {
      if (currentPhase == GamePhase.PhaseThree) return;
      SetPhase(currentPhase + 1);
    }

    public void RegressPhase()
    {
      if (currentPhase == GamePhase.PhaseOne) return;
      SetPhase(currentPhase - 1);
    }

        void ApplyPhase(GamePhase phase)
        {
            //Debug.Log($"ApplyPhase: {phase}");

            switch (phase)
            {
                case GamePhase.PhaseOne:
                    inputRouter.autoFire = true;
                    SetKickerPhase(leftKicker, graduated: false, minStrength: 0f, popOnNoInput: false);
                    SetKickerPhase(rightKicker, graduated: false, minStrength: 0f, popOnNoInput: false);
                    break;

                case GamePhase.PhaseTwo:
                    inputRouter.autoFire = false;
                    SetKickerPhase(leftKicker, graduated: true, minStrength: phaseTwoMinStrength, popOnNoInput: false);
                    SetKickerPhase(rightKicker, graduated: true, minStrength: phaseTwoMinStrength, popOnNoInput: false);
                    break;

                case GamePhase.PhaseThree:
                    inputRouter.autoFire = false;
                    SetKickerPhase(leftKicker, graduated: true, minStrength: 0f, popOnNoInput: true);
                    SetKickerPhase(rightKicker, graduated: true, minStrength: 0f, popOnNoInput: true);
                    break;
            }

/*            if (leftKicker != null)
                Debug.Log($"Left kicker — graduated: {leftKicker.graduatedForce}, minStrength: {leftKicker.minStrength}, popOnNoInput: {leftKicker.popOnNoInput}");
            if (rightKicker != null)
                Debug.Log($"Right kicker — graduated: {rightKicker.graduatedForce}, minStrength: {rightKicker.minStrength}, popOnNoInput: {rightKicker.popOnNoInput}");
            if (inputRouter != null)
                Debug.Log($"InputRouter — autoFire: {inputRouter.autoFire}");*/
        }

        void SetKickerPhase(KickerForce kicker, bool graduated, float minStrength, bool popOnNoInput)
    {
      if (kicker == null) return;
      kicker.graduatedForce = graduated;
      kicker.minStrength = minStrength;
      kicker.popOnNoInput = popOnNoInput;
    }

    void HandleAutoAdvance()
    {
      if (GameManager.Instance == null) return;

      if (autoAdvanceFromPhaseOne && currentPhase == GamePhase.PhaseOne)
      {
        bool ballCondition = phaseTwoBallThreshold > 0 &&
                             GameManager.Instance.spawnIndex >= phaseTwoBallThreshold;
        bool scoreCondition = phaseTwoScoreThreshold > 0 &&
                              GameManager.Instance.CurrentScore >= phaseTwoScoreThreshold;
        if (ballCondition || scoreCondition)
          AdvancePhase();
      }

      if (autoAdvanceFromPhaseTwo && currentPhase == GamePhase.PhaseTwo)
      {
        bool ballCondition = phaseThreeBallThreshold > 0 &&
                             GameManager.Instance.spawnIndex >= phaseThreeBallThreshold;
        bool scoreCondition = phaseThreeScoreThreshold > 0 &&
                              GameManager.Instance.CurrentScore >= phaseThreeScoreThreshold;
        if (ballCondition || scoreCondition)
          AdvancePhase();
      }
    }

    void OnGUI()
    {
      if (!showDebugUI) return;

      GUILayout.BeginArea(new Rect(10, 10, 200, 120));
      GUILayout.Label($"Current Phase: {currentPhase}");
      if (GUILayout.Button("Phase 1")) SetPhase(GamePhase.PhaseOne);
      if (GUILayout.Button("Phase 2")) SetPhase(GamePhase.PhaseTwo);
      if (GUILayout.Button("Phase 3")) SetPhase(GamePhase.PhaseThree);
      GUILayout.EndArea();
    }
  }
}