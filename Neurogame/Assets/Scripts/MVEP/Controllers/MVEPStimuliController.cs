using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the pulsing of MVEP stimuli in a randomized order with a specified interval.
/// 
/// I have written this script to use arrays of StimulusControllers due to arrays being slightly more performant than lists, but we can change this if needed for variable list sizes.
/// </summary>
public class MVEPStimuliController : MonoBehaviour
{
  [Header("Component References")]
  [SerializeField] private MVEPStimulus[] mvepStimuli;

  [Header("Settings")]
  [SerializeField] private float pulseInterval = 0.2f;

  public void Pulse()
  {
    StartCoroutine(PulseStimuli(ShuffleStimuli()));
  }

  private MVEPStimulus[] ShuffleStimuli()
  {
    MVEPStimulus[] shuffled = new MVEPStimulus[mvepStimuli.Length];
    mvepStimuli.CopyTo(shuffled, 0);
    for (int i = 0; i < shuffled.Length; i++)
    {
      MVEPStimulus temp = shuffled[i];
      int randomIndex = Random.Range(i, shuffled.Length);
      shuffled[i] = shuffled[randomIndex];
      shuffled[randomIndex] = temp;
    }
    return shuffled;
  }

  private IEnumerator PulseStimuli(MVEPStimulus[] stimuli)
  {
    foreach (var stimulus in stimuli)
    {
      stimulus.Pulse();
      yield return new WaitForSeconds(pulseInterval);
    }
  }
}
