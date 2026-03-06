using UnityEngine;
using UnityEngine.UI;

public class NeuroMeterUI : MonoBehaviour
{
    public Image neuroMeter;   // blue filled image (vertical)
    public Image chargeMeter;  // charge filled image (vertical)

    public RectTransform thresholdBand;
    public RectTransform neuroContainer;

    private void Awake()
    {
        if (neuroMeter) neuroMeter.fillAmount = 0f;
        if (chargeMeter) chargeMeter.fillAmount = 0f;
    }

    public void SetNeuroValue(float v) => neuroMeter.fillAmount = Mathf.Clamp01(v);
    public void SetChargeValue(float v) => chargeMeter.fillAmount = Mathf.Clamp01(v);

    public void SetThreshold(float min, float max)
    {
        if (thresholdBand == null || neuroContainer == null) return;

        min = Mathf.Clamp01(min);
        max = Mathf.Clamp01(max);

        float height = neuroContainer.rect.height;
        float y = min * height;
        float h = Mathf.Max(2f, (max - min) * height);

        thresholdBand.anchorMin = new Vector2(0f, 0f);
        thresholdBand.anchorMax = new Vector2(1f, 0f);
        thresholdBand.pivot = new Vector2(0.5f, 0f);

        thresholdBand.offsetMin = new Vector2(0f, thresholdBand.offsetMin.y);
        thresholdBand.offsetMax = new Vector2(0f, thresholdBand.offsetMax.y);

        thresholdBand.anchoredPosition = new Vector2(0f, y);
        thresholdBand.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }
}