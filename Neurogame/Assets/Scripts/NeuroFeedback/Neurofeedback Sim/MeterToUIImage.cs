using UnityEngine;
using UnityEngine.UI;

public class MeterToUIImage : MonoBehaviour
{
    [SerializeField] private NeuroMeter meter;
    [SerializeField] private Image fillImage;
    [SerializeField] private float smooth = 12f;

    private float v;

    void Awake()
    {
        if (fillImage != null) v = fillImage.fillAmount;
    }

    void Update()
    {
        if (meter == null || fillImage == null) return;

        float target = meter.meterValue;
        float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        v = Mathf.Lerp(v, target, k);
        fillImage.fillAmount = Mathf.Clamp01(v);
    }
}
