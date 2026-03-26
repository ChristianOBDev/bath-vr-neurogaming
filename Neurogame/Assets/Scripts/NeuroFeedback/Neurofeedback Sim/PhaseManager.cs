using UnityEngine;

namespace NeuroFeedback
{
    public class PhaseManager : MonoBehaviour
    {
        public enum Phase
        {
            Phase1_Baseline,
            Phase2_Assisted,
            Phase3
        }

        [Header("Current Phase")]
        [SerializeField] private Phase currentPhase = Phase.Phase1_Baseline;

        [Header("Mode")]
        [Tooltip("When true, the phase is controlled by the UI buttons.")]
        public bool useManualPhaseSelection = true;

        public Phase CurrentPhase => currentPhase;

        public void SetManualPhase(Phase newPhase)
        {
            if (!useManualPhaseSelection)
                return;

            currentPhase = newPhase;
        }

        public void SetPhase1()
        {
            if (!useManualPhaseSelection)
                return;

            currentPhase = Phase.Phase1_Baseline;
        }

        public void SetPhase2()
        {
            if (!useManualPhaseSelection)
                return;

            currentPhase = Phase.Phase2_Assisted;
        }

        public void SetPhase3()
        {
            if (!useManualPhaseSelection)
                return;

            currentPhase = Phase.Phase3;
        }
    }
}