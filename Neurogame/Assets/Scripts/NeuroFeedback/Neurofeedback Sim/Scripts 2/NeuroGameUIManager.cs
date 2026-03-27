using UnityEngine;

namespace NeuroFeedback
{
    public class NeuroMenuManager : MonoBehaviour
    {
        [Header("Gameplay References")]
        public NeuroChargeController neuroChargeController;
        public PhaseManager phaseManager;
        public PhaseGatedContinuousScore scoreManager;

        [Header("UI Object Switching")]
        public GameObject mainMenuObject;
        public GameObject phaseSelectorObject;

        private void Awake()
        {
            if (scoreManager == null)
                scoreManager = PhaseGatedContinuousScore.Instance;
        }

        public void StartGame()
        {
            if (neuroChargeController != null)
                neuroChargeController.BeginSession();
        }

        public void PauseGame()
        {
            if (neuroChargeController != null)
                neuroChargeController.PauseSession();
        }

        public void ResumeGame()
        {
            if (neuroChargeController != null)
                neuroChargeController.ResumeSession();
        }

        public void RestartGame()
        {
            // Stop current local session
            if (neuroChargeController != null)
            {
                neuroChargeController.StopSession();
                neuroChargeController.ResetCharge();
                neuroChargeController.ManualRandomizeThreshold();
            }

            // Reset score only for this minigame
            if (scoreManager == null)
                scoreManager = PhaseGatedContinuousScore.Instance;

            if (scoreManager != null)
                scoreManager.ResetScore();

            // Reset phase back to the first phase
            if (phaseManager != null)
                phaseManager.SetPhase1();

            // Start again from the beginning
            if (neuroChargeController != null)
                neuroChargeController.BeginSession();
        }

        public void OpenPhaseSelector()
        {
            if (mainMenuObject != null)
                mainMenuObject.SetActive(false);

            if (phaseSelectorObject != null)
                phaseSelectorObject.SetActive(true);
        }

        public void BackFromPhaseSelector()
        {
            if (phaseSelectorObject != null)
                phaseSelectorObject.SetActive(false);

            if (mainMenuObject != null)
                mainMenuObject.SetActive(true);
        }

        public void SelectPhase1()
        {
            if (phaseManager != null)
                phaseManager.SetPhase1();

            ApplyPhaseSelection();
        }

        public void SelectPhase2()
        {
            if (phaseManager != null)
                phaseManager.SetPhase2();

            ApplyPhaseSelection();
        }

        public void SelectPhase3()
        {
            if (phaseManager != null)
                phaseManager.SetPhase3();

            ApplyPhaseSelection();
        }

        private void ApplyPhaseSelection()
        {
            if (phaseSelectorObject != null)
                phaseSelectorObject.SetActive(false);

            if (mainMenuObject != null)
                mainMenuObject.SetActive(true);

            if (neuroChargeController != null)
            {
                neuroChargeController.StopSession();
                neuroChargeController.ResetCharge();
                neuroChargeController.ManualRandomizeThreshold();
            }
        }
    }
}