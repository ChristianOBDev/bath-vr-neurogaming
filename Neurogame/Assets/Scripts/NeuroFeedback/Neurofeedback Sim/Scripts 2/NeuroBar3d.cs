using UnityEngine;

public class NeuroBar3D : MonoBehaviour
{
    [Header("Bar Settings")]
    public Transform barTransform; // The part of the bar that fills
    public Vector3 fillDirection = Vector3.down; // Direction to fill (down, up, left, right)
    public float minY = -1f; // Min position when empty
    public float maxY = 1f;  // Max position when full

    [Header("Materials")]
    public Material baseMaterial; // Material for the empty part
    public Material fillMaterial;  // Material for the filled part
    public Material thresholdMaterial; // Material for threshold band
    public Renderer barRenderer; // Renderer to apply materials

    [Header("Threshold Band")]
    public Transform thresholdBand; // A thin band/plane to show threshold
    public float bandThickness = 0.1f;
    public Vector3 bandOffset = Vector3.zero; // Adjust position if needed

    [Header("Current Values")]
    [Range(0f, 1f)]
    public float neuroValue = 0f;
    [Range(0f, 1f)]
    public float chargeValue = 0f;
    [Range(0f, 1f)]
    public float thresholdMin = 0.2f;
    [Range(0f, 1f)]
    public float thresholdMax = 0.8f;

    [Header("Visual Options")]
    public bool showNeuroFill = true; // If false, shows charge fill
    public Gradient thresholdGradient = new Gradient();

    private MaterialPropertyBlock propBlock;
    private Vector3 initialBarPosition;
    private Vector3 initialBandPosition;
    private float barHeight;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        // Store initial positions
        if (barTransform != null)
            initialBarPosition = barTransform.localPosition;

        if (thresholdBand != null)
            initialBandPosition = thresholdBand.localPosition;

        // Calculate bar height based on min/max range
        barHeight = Mathf.Abs(maxY - minY);

        // Setup default gradient if not set
        if (thresholdGradient.colorKeys.Length == 0)
        {
            thresholdGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.green, 0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(Color.red, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }
    }

    private void Update()
    {
        UpdateBarPosition();
        UpdateThresholdBand();
        UpdateMaterials();
    }

    private void UpdateBarPosition()
    {
        if (barTransform == null) return;

        float value = showNeuroFill ? neuroValue : chargeValue;
        float targetY = Mathf.Lerp(minY, maxY, value);

        Vector3 newPos = initialBarPosition;

        // Apply fill direction
        if (fillDirection == Vector3.up)
            newPos.y = targetY;
        else if (fillDirection == Vector3.down)
            newPos.y = -targetY;
        else if (fillDirection == Vector3.right)
            newPos.x = targetY;
        else if (fillDirection == Vector3.left)
            newPos.x = -targetY;
        else if (fillDirection == Vector3.forward)
            newPos.z = targetY;
        else if (fillDirection == Vector3.back)
            newPos.z = -targetY;

        barTransform.localPosition = newPos;
    }

    private void UpdateThresholdBand()
    {
        if (thresholdBand == null) return;

        // Position band at threshold min
        float thresholdPos = Mathf.Lerp(minY, maxY, thresholdMin);

        Vector3 newPos = initialBandPosition;

        if (fillDirection == Vector3.up || fillDirection == Vector3.down)
            newPos.y = thresholdPos;
        else if (fillDirection == Vector3.right || fillDirection == Vector3.left)
            newPos.x = thresholdPos;
        else if (fillDirection == Vector3.forward || fillDirection == Vector3.back)
            newPos.z = thresholdPos;

        // Add offset
        newPos += bandOffset;
        thresholdBand.localPosition = newPos;

        // Scale band to show threshold range
        float bandRange = thresholdMax - thresholdMin;
        float bandScale = bandRange * barHeight;

        Vector3 scale = thresholdBand.localScale;

        if (fillDirection == Vector3.up || fillDirection == Vector3.down)
            scale.y = bandThickness;
        else if (fillDirection == Vector3.right || fillDirection == Vector3.left)
            scale.x = bandThickness;
        else if (fillDirection == Vector3.forward || fillDirection == Vector3.back)
            scale.z = bandThickness;

        thresholdBand.localScale = scale;

        // Update threshold band color based on position/value
        if (thresholdBand.TryGetComponent<Renderer>(out var bandRenderer))
        {
            float t = Mathf.InverseLerp(minY, maxY, thresholdPos);
            bandRenderer.material.color = thresholdGradient.Evaluate(t);
        }
    }

    private void UpdateMaterials()
    {
        if (barRenderer == null || baseMaterial == null || fillMaterial == null) return;

        // Apply materials based on fill level
        // This is a simple approach - for more complex shader effects,
        // you might want to use a custom shader with fill amount property

        float fillAmount = showNeuroFill ? neuroValue : chargeValue;

        // Get current property block or create new
        barRenderer.GetPropertyBlock(propBlock);

        // Set fill amount property if shader supports it
        propBlock.SetFloat("_FillAmount", fillAmount);
        propBlock.SetFloat("_ThresholdMin", thresholdMin);
        propBlock.SetFloat("_ThresholdMax", thresholdMax);

        // Set colors based on values
        Color fillColor = showNeuroFill ? Color.cyan : Color.yellow;
        propBlock.SetColor("_FillColor", fillColor);

        barRenderer.SetPropertyBlock(propBlock);
    }

    // Public methods to update values (to be called from NeuroChargeController)
    public void SetNeuroValue(float value)
    {
        neuroValue = Mathf.Clamp01(value);
    }

    public void SetChargeValue(float value)
    {
        chargeValue = Mathf.Clamp01(value);
    }

    public void SetThreshold(float min, float max)
    {
        thresholdMin = Mathf.Clamp01(min);
        thresholdMax = Mathf.Clamp01(max);
    }

    public void SetShowNeuroFill(bool showNeuro)
    {
        showNeuroFill = showNeuro;
    }

    // Immediate update for bar position (useful for resets)
    public void SetValueImmediate(float value, bool isNeuro = true)
    {
        if (isNeuro)
            neuroValue = Mathf.Clamp01(value);
        else
            chargeValue = Mathf.Clamp01(value);

        UpdateBarPosition();
    }
}
