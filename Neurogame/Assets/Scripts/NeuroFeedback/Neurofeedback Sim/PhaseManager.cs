using UnityEngine;

namespace NeuroFeedback
{
    public class PhaseManager : MonoBehaviour
    {
        public enum Phase { Phase1_Baseline, Phase2_Assisted, Phase3_Full }
        public Phase CurrentPhase = Phase.Phase1_Baseline;

        [Header("Auto Advance (optional)")]
        public bool autoAdvance = false;
        public float phase1Duration = 30f;
        public float phase2Duration = 60f;

        private float t;

        void Update()
        {
            if (!autoAdvance) return;

            t += Time.deltaTime;

            if (CurrentPhase == Phase.Phase1_Baseline && t >= phase1Duration)
            {
                CurrentPhase = Phase.Phase2_Assisted;
                t = 0f;
                Debug.Log("[PHASE] -> Phase 2 Assisted");
            }
            else if (CurrentPhase == Phase.Phase2_Assisted && t >= phase2Duration)
            {
                CurrentPhase = Phase.Phase3_Full;
                t = 0f;
                Debug.Log("[PHASE] -> Phase 3 Full");
            }
        }
    }
}