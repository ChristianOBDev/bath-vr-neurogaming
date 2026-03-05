using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class MVEPStimuliController : MonoBehaviour
{
  [Header("Component References")]
  [SerializeField] private MVEPStimulus[] mvepStimuli;

  [Header("Settings")]
  private float pulseInterval = 0.2f;
  private float p300Interval = 0.3f;

  public static Action PulseComplete;

  void Awake()
  {
    pulseInterval = MVEPGameSettings.Instance.mvepConfig.PulseInterval;
    p300Interval = MVEPGameSettings.Instance.mvepConfig.p300Interval;
  }

  public void Pulse(float delay = 0f)
  {
    StartCoroutine(PulseStimuli(ShuffleStimuli(), delay));
  }

  private MVEPStimulus[] ShuffleStimuli()
  {
    MVEPStimulus[] shuffled = new MVEPStimulus[mvepStimuli.Length];
    mvepStimuli.CopyTo(shuffled, 0);
    for (int i = 0; i < shuffled.Length; i++)
    {
      MVEPStimulus temp = shuffled[i];
      int randomIndex = UnityEngine.Random.Range(i, shuffled.Length);
      shuffled[i] = shuffled[randomIndex];
      shuffled[randomIndex] = temp;
    }
    return shuffled;
  }

  private IEnumerator PulseStimuli(MVEPStimulus[] stimuli, float delay = 0f)
  {
    if (delay > 0f)
    {
      yield return new WaitForSeconds(delay);
    }

    foreach (var stimulus in stimuli)
    {
      stimulus.Pulse();
      yield return new WaitForSeconds(pulseInterval);
    }

    yield return new WaitForSeconds(p300Interval);

    PulseComplete?.Invoke();
  }
}
