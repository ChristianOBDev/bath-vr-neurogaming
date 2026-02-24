using UnityEngine;

[CreateAssetMenu(fileName = "MVEPConfig", menuName = "ScriptableObjects/MVEPConfig", order = 2)]
public class MVEPConfiguration : ScriptableObject
{
  [Header("Stimuli Settings")]
  public int numStimuli = 5;
  public float arrowPreactivationTime = 1f;
  public float pulseDuration = 0.14f;
  public float pulseOffset = 0.06f;
  public float PulseInterval => pulseDuration + pulseOffset;
  public float WaveDuration => numStimuli * PulseInterval;
  public float p300Interval = 0.3f;
  public float StimuliDuration => arrowPreactivationTime + WaveDuration + p300Interval;
}
