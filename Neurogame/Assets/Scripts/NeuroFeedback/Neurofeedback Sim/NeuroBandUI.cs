using UnityEngine;
using UnityEngine.UI;

public class NeuroBandUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private NeuroBandTrainer trainer;

    [Header("Brain Meter UI (Signal)")]
    [SerializeField] private Image brainFill;
    [SerializeField] private RectTransform brainBarRect;
    [SerializeField] private RectTransform bandWindowRect;

    [Header("Charge Meter UI (Charge/Accuracy)")]
    [SerializeField] private Image chargeFill;

    [Header("Smoothing")]
    public float smooth = 14f;

    private float brainV, chargeV;

    void Start()
    {
        // ✅ prevent “full by default”
        if (brainFill != null) brainFill.fillAmount = 0f;
        if (chargeFill != null) chargeFill.fillAmount = 0f;

        // ✅ ensure red band is enabled
        if (bandWindowRect != null)
            bandWindowRect.gameObject.SetActive(true);
    }

    void Update()
    {
        if (trainer == null) return;

        if (brainFill != null)
        {
            float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            brainV = Mathf.Lerp(brainV, trainer.brain01, k);
            brainFill.fillAmount = Mathf.Clamp01(brainV);
        }

        if (chargeFill != null)
        {
            float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            chargeV = Mathf.Lerp(chargeV, trainer.charge01, k);
            chargeFill.fillAmount = Mathf.Clamp01(chargeV);
        }

        if (brainBarRect != null && bandWindowRect != null)
        {
            float h = brainBarRect.rect.height;
            float yMin = -h * 0.5f;
            float yMax = +h * 0.5f;

            // stationary band position
            float yCenter = Mathf.Lerp(yMin, yMax, trainer.bandCenter01);

            // band height based on width (percentage of bar height)
            float windowH = Mathf.Max(8f, h * trainer.bandWidth);
            bandWindowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowH);

            // move it
            Vector2 p = bandWindowRect.anchoredPosition;
            p.y = yCenter;
            bandWindowRect.anchoredPosition = p;
        }
    }
}