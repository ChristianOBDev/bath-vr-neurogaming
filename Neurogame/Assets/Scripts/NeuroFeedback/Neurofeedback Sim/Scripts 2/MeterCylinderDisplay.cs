using UnityEngine;

public class PillarDualCubeMeter : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [System.Serializable]
    public class VerticalMeter
    {
        [Header("Drop these 2 cubes")]
        public Transform baseCube;
        public Transform fillCube;

        [Header("Materials (Neuro only)")]
        public Material baseMaterial;
        public Material fillMaterial;

        [Header("Fill Direction")]
        [Tooltip("Choose the LOCAL axis of the fill cube that represents its vertical height.")]
        public Axis fillAxis = Axis.Y;

        [Header("Animation")]
        public float lerpSpeed = 10f;

        [HideInInspector] public float targetValue;
        [HideInInspector] public float currentValue;

        [HideInInspector] public Transform fillPivot;
        [HideInInspector] public Renderer baseRenderer;
        [HideInInspector] public Renderer fillRenderer;

        [HideInInspector] public float worldBottom;
        [HideInInspector] public float worldTop;
        [HideInInspector] public float fullHeight;
    }

    [System.Serializable]
    public class HorizontalMeter
    {
        [Header("Drop these 2 cubes")]
        public Transform baseCube;
        public Transform fillCube;

        [Header("Animation")]
        public float lerpSpeed = 10f;

        [Header("Charge Color")]
        public Renderer fillRenderer;
        public bool useHueShift = true;

        [Tooltip("Color for low range")]
        public Color lowColor = Color.red;

        [Tooltip("Color for mid range")]
        public Color midColor = new Color(1f, 0.5f, 0f);

        [Tooltip("Color for high range")]
        public Color highColor = Color.green;

        [Header("Color Thresholds")]
        [Range(0f, 1f)] public float redToOrangeAt = 0.50f;
        [Range(0f, 1f)] public float orangeToGreenAt = 0.85f;

        [HideInInspector] public Renderer baseRenderer;
        [HideInInspector] public Transform fillPivot;
        [HideInInspector] public float targetValue;
        [HideInInspector] public float currentValue;
        [HideInInspector] public Material runtimeMaterial;
    }

    [Header("Neuro Meter (Bottom -> Top)")]
    public VerticalMeter neuroMeter;

    [Header("Charge Meter (Left -> Right)")]
    public HorizontalMeter chargeMeter;

    [Header("Threshold Cube")]
    public Transform thresholdCube;
    public float thresholdYOffset = 0f;

    [Header("Threshold Range")]
    [Range(0f, 1f)] public float thresholdMin01 = 0.4f;
    [Range(0f, 1f)] public float thresholdMax01 = 0.6f;

    [Header("Threshold Colors")]
    public Color thresholdInsideColor = Color.green;
    public Color thresholdOutsideColor = Color.red;

    private Renderer thresholdRenderer;
    private Material thresholdRuntimeMaterial;

    private void Awake()
    {
        SetupVerticalMeter(neuroMeter);
        SetupHorizontalMeter(chargeMeter);
        SetupThresholdCube();

        SetNeuroValueImmediate(0f);
        SetChargeValueImmediate(0f);
        SetThreshold(thresholdMin01, thresholdMax01);
        UpdateThresholdColor(0f);
    }

    private void Update()
    {
        TickVerticalMeter(neuroMeter);
        TickHorizontalMeter(chargeMeter);
        UpdateThresholdColor(neuroMeter != null ? neuroMeter.currentValue : 0f);
    }

    private void SetupVerticalMeter(VerticalMeter meter)
    {
        if (meter == null || meter.baseCube == null || meter.fillCube == null)
            return;

        meter.baseRenderer = meter.baseCube.GetComponent<Renderer>();
        meter.fillRenderer = meter.fillCube.GetComponent<Renderer>();

        if (meter.baseRenderer == null)
        {
            Debug.LogWarning($"[PillarDualCubeMeter] Neuro base cube '{meter.baseCube.name}' has no Renderer.");
            return;
        }

        if (meter.fillRenderer == null)
        {
            Debug.LogWarning($"[PillarDualCubeMeter] Neuro fill cube '{meter.fillCube.name}' has no Renderer.");
            return;
        }

        if (meter.baseMaterial != null)
            meter.baseRenderer.material = meter.baseMaterial;

        if (meter.fillMaterial != null)
            meter.fillRenderer.material = meter.fillMaterial;

        meter.baseCube.gameObject.SetActive(true);

        Bounds baseBounds = meter.baseRenderer.bounds;
        meter.worldBottom = baseBounds.min.y;
        meter.worldTop = baseBounds.max.y;
        meter.fullHeight = baseBounds.size.y;

        Bounds fillBounds = meter.fillRenderer.bounds;
        Vector3 bottomWorld = new Vector3(fillBounds.center.x, fillBounds.min.y, fillBounds.center.z);

        Transform originalParent = meter.fillCube.parent;

        GameObject pivotGO = new GameObject(meter.fillCube.name + "_BottomPivot");
        Transform pivot = pivotGO.transform;

        pivot.SetParent(originalParent, true);
        pivot.position = bottomWorld;
        pivot.rotation = originalParent != null ? originalParent.rotation : Quaternion.identity;
        pivot.localScale = Vector3.one;

        meter.fillCube.SetParent(pivot, true);
        meter.fillPivot = pivot;

        meter.targetValue = 0f;
        meter.currentValue = 0f;
    }

    private void SetupHorizontalMeter(HorizontalMeter meter)
    {
        if (meter == null || meter.baseCube == null || meter.fillCube == null)
            return;

        meter.baseRenderer = meter.baseCube.GetComponent<Renderer>();

        if (meter.baseRenderer == null)
        {
            Debug.LogWarning($"[PillarDualCubeMeter] Charge base cube '{meter.baseCube.name}' has no Renderer.");
            return;
        }

        if (meter.fillRenderer == null)
            meter.fillRenderer = meter.fillCube.GetComponent<Renderer>();

        if (meter.fillRenderer == null)
        {
            Debug.LogWarning($"[PillarDualCubeMeter] Charge fill cube '{meter.fillCube.name}' has no Renderer.");
            return;
        }

        meter.runtimeMaterial = meter.fillRenderer.material;
        meter.baseCube.gameObject.SetActive(true);

        Transform originalParent = meter.fillCube.parent;

        Bounds b = meter.fillRenderer.bounds;
        Vector3 leftWorld = new Vector3(b.center.x, b.center.y, b.min.z);

        GameObject pivotGO = new GameObject(meter.fillCube.name + "_LeftPivot");
        Transform pivot = pivotGO.transform;

        pivot.SetParent(originalParent, true);
        pivot.position = leftWorld;
        pivot.rotation = meter.fillCube.rotation;
        pivot.localScale = Vector3.one;

        meter.fillCube.SetParent(pivot, true);

        meter.fillPivot = pivot;
        meter.targetValue = 0f;
        meter.currentValue = 0f;

        UpdateChargeColor(meter, 0f);
    }

    private void SetupThresholdCube()
    {
        if (thresholdCube == null) return;

        thresholdRenderer = thresholdCube.GetComponent<Renderer>();
        if (thresholdRenderer == null)
        {
            Debug.LogWarning($"[PillarDualCubeMeter] Threshold cube '{thresholdCube.name}' has no Renderer.");
            return;
        }

        thresholdRuntimeMaterial = thresholdRenderer.material;
    }

    private void TickVerticalMeter(VerticalMeter meter)
    {
        if (meter == null || meter.fillPivot == null) return;

        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, meter.lerpSpeed) * Time.deltaTime);
        meter.currentValue = Mathf.Lerp(meter.currentValue, meter.targetValue, k);
        meter.currentValue = Mathf.Clamp01(meter.currentValue);

        ApplyVerticalFill(meter, meter.currentValue);
    }

    private void TickHorizontalMeter(HorizontalMeter meter)
    {
        if (meter == null || meter.fillPivot == null) return;

        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, meter.lerpSpeed) * Time.deltaTime);
        meter.currentValue = Mathf.Lerp(meter.currentValue, meter.targetValue, k);
        meter.currentValue = Mathf.Clamp01(meter.currentValue);

        ApplyHorizontalFill(meter, meter.currentValue);
        UpdateChargeColor(meter, meter.currentValue);
    }

    private void ApplyVerticalFill(VerticalMeter meter, float value01)
    {
        if (meter.fillPivot == null) return;

        value01 = Mathf.Clamp01(value01);

        Vector3 s = meter.fillPivot.localScale;

        switch (meter.fillAxis)
        {
            case Axis.X:
                s.x = value01;
                break;
            case Axis.Y:
                s.y = value01;
                break;
            case Axis.Z:
                s.z = value01;
                break;
        }

        meter.fillPivot.localScale = s;
        meter.fillPivot.gameObject.SetActive(value01 > 0.0001f);
    }

    private void ApplyHorizontalFill(HorizontalMeter meter, float value01)
    {
        if (meter.fillPivot == null) return;

        value01 = Mathf.Clamp01(value01);

        Vector3 s = meter.fillPivot.localScale;
        s.z = value01;
        meter.fillPivot.localScale = s;

        meter.fillPivot.gameObject.SetActive(value01 > 0.0001f);
    }

    private void UpdateChargeColor(HorizontalMeter meter, float normalized01)
    {
        if (!meter.useHueShift || meter.runtimeMaterial == null) return;

        normalized01 = Mathf.Clamp01(normalized01);

        float redEnd = Mathf.Clamp01(meter.redToOrangeAt);
        float greenStart = Mathf.Clamp(meter.orangeToGreenAt, redEnd + 0.01f, 1f);

        Color c;

        if (normalized01 <= redEnd)
        {
            c = meter.lowColor;
        }
        else if (normalized01 <= greenStart)
        {
            c = meter.midColor;
        }
        else
        {
            c = meter.highColor;
        }

        if (meter.runtimeMaterial.HasProperty("_BaseColor"))
            meter.runtimeMaterial.SetColor("_BaseColor", c);

        if (meter.runtimeMaterial.HasProperty("_Color"))
            meter.runtimeMaterial.SetColor("_Color", c);

        if (meter.runtimeMaterial.HasProperty("_EmissionColor"))
            meter.runtimeMaterial.SetColor("_EmissionColor", c);
    }

    private void UpdateThresholdColor(float neuroValue01)
    {
        if (thresholdRuntimeMaterial == null) return;

        neuroValue01 = Mathf.Clamp01(neuroValue01);

        float min = Mathf.Min(thresholdMin01, thresholdMax01);
        float max = Mathf.Max(thresholdMin01, thresholdMax01);

        bool inside = neuroValue01 >= min && neuroValue01 <= max;
        Color c = inside ? thresholdInsideColor : thresholdOutsideColor;

        if (thresholdRuntimeMaterial.HasProperty("_BaseColor"))
            thresholdRuntimeMaterial.SetColor("_BaseColor", c);

        if (thresholdRuntimeMaterial.HasProperty("_Color"))
            thresholdRuntimeMaterial.SetColor("_Color", c);

        if (thresholdRuntimeMaterial.HasProperty("_EmissionColor"))
            thresholdRuntimeMaterial.SetColor("_EmissionColor", c);
    }

    public void SetNeuroValue(float value01)
    {
        if (neuroMeter != null)
            neuroMeter.targetValue = Mathf.Clamp01(value01);
    }

    public void SetChargeValue(float value01)
    {
        if (chargeMeter != null)
            chargeMeter.targetValue = Mathf.Clamp01(value01);
    }

    public void SetNeuroValueImmediate(float value01)
    {
        if (neuroMeter == null) return;

        value01 = Mathf.Clamp01(value01);
        neuroMeter.targetValue = value01;
        neuroMeter.currentValue = value01;
        ApplyVerticalFill(neuroMeter, value01);
        UpdateThresholdColor(value01);
    }

    public void SetChargeValueImmediate(float value01)
    {
        if (chargeMeter == null) return;

        value01 = Mathf.Clamp01(value01);
        chargeMeter.targetValue = value01;
        chargeMeter.currentValue = value01;
        ApplyHorizontalFill(chargeMeter, value01);
        UpdateChargeColor(chargeMeter, value01);
    }

    public void SetThreshold(float min01, float max01)
    {
        if (thresholdCube == null || neuroMeter == null) return;
        if (neuroMeter.fullHeight <= 0f) return;

        thresholdMin01 = Mathf.Clamp01(min01);
        thresholdMax01 = Mathf.Clamp01(max01);

        float center01 = (thresholdMin01 + thresholdMax01) * 0.5f;
        float worldY = neuroMeter.worldBottom + neuroMeter.fullHeight * center01;

        Vector3 worldPos = thresholdCube.position;
        worldPos.y = worldY + thresholdYOffset;

        Transform parent = thresholdCube.parent;
        if (parent != null)
            thresholdCube.localPosition = parent.InverseTransformPoint(worldPos);
        else
            thresholdCube.position = worldPos;

        UpdateThresholdColor(neuroMeter.currentValue);
    }
}