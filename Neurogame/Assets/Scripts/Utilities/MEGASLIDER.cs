using UnityEngine;
using UnityEngine.Audio;

public class MegaSlider : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Mixer Parameter Names")]
    public string sliderStateParam = "sliderState";
    public string rnboVolParam = "RNBOvol";
    public string focusedVolParam = "FocusedVol";

    [Header("VFX")]
    public GameObject vfxManagerPos;

    [Header("UI")]
    [Range(0f, 1f)]
    [Tooltip("0 = relaxed, 1 = focused")]
    public float megaSliderValue = 0f;

    private float _lastApplied = -1f;

    private const float RnboMinLinear = 0.5f;
    private const float RnboMaxLinear = 1f;
    private const float SilenceDb = -80f;

    void Update()
    {
        if (!Mathf.Approximately(megaSliderValue, _lastApplied))
        {
            ApplyMegaSlider(megaSliderValue);
        }
    }

    public void ApplyMegaSlider(float t)
    {
        t = Mathf.Clamp01(t);
        megaSliderValue = t;
        _lastApplied = t;

        // --- Mixer params (exposed check) ---
        if (mixer == null)
        {
            Debug.LogError("MegaSlider: AudioMixer not assigned.");
            return;
        }

        if (!mixer.SetFloat(sliderStateParam, t))
            Debug.LogWarning($"MegaSlider: '{sliderStateParam}' not exposed on mixer.");

        float focusedT = t > 0.6f ? (t - 0.6f) / 0.4f : 0f;
        float focusedDb = focusedT > 0.0001f ? 20f * Mathf.Log10(focusedT) : SilenceDb;
        if (!mixer.SetFloat(focusedVolParam, focusedDb))
            Debug.LogWarning($"MegaSlider: '{focusedVolParam}' not exposed on mixer.");

        float rnboLinear = Mathf.Lerp(RnboMaxLinear, RnboMinLinear, t);
        float rnboDb = rnboLinear > 0.0001f ? 20f * Mathf.Log10(rnboLinear) : SilenceDb;
        if (!mixer.SetFloat(rnboVolParam, rnboDb))
            Debug.LogWarning($"MegaSlider: '{rnboVolParam}' not exposed on mixer.");

        // --- VFX override ---
        if (vfxManagerPos == null)
        {
            Debug.LogWarning("MegaSlider: vfxManagerPos not assigned.");
            return;
        }

        if (vfxManagerPos.TryGetComponent<BCIStateController>(out var bci))
            bci.manualOverride = t;
        else
            Debug.LogWarning("MegaSlider: BCIStateController not found on vfxManagerPos.");
    }
}