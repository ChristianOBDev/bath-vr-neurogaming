using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeuroFeedback
{
    public class NeuroMenuManager : MonoBehaviour
    {
        [Header("Optional Gameplay Reference")]
        public NeuroChargeController neuroChargeController;

        [Header("Manual Phase Reference")]
        public PhaseManager phaseManager;

        [Header("UI Object Switching")]
        public GameObject mainMenuObject;
        public GameObject phaseSelectorObject;

        [Header("Options")]
        public bool pauseAudioListener = true;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (pauseAudioListener)
                AudioListener.pause = false;
        }

        public void StartGame()
        {
            Time.timeScale = 1f;

            if (pauseAudioListener)
                AudioListener.pause = false;

            if (neuroChargeController != null)
                neuroChargeController.BeginSession();
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;

            if (pauseAudioListener)
                AudioListener.pause = true;

            if (neuroChargeController != null)
                neuroChargeController.PauseSession();
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;

            if (pauseAudioListener)
                AudioListener.pause = false;

            if (neuroChargeController != null)
                neuroChargeController.ResumeSession();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

            if (pauseAudioListener)
                AudioListener.pause = false;

            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
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