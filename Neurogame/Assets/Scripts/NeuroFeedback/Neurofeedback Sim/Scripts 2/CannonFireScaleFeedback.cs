using UnityEngine;

public class CannonChargeVisualFeedback : MonoBehaviour
{
    [Header("Charge Source Visuals")]
    [Tooltip("The transform for the fire visual that grows with charge.")]
    public Transform fireVisual;

    [Tooltip("The transform for the cannon barrel that tilts with charge.")]
    public Transform barrelPivot;

    [Header("Fire Scale")]
    [Tooltip("Scale when charge is 0.")]
    public Vector3 minFireScale = new Vector3(0.2f, 0.2f, 0.2f);

    [Tooltip("Scale when charge is 1.")]
    public Vector3 maxFireScale = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("Barrel Rotation")]
    [Tooltip("Usually X for cannon elevation.")]
    public Vector3 barrelLocalRotationAxis = Vector3.right;

    [Tooltip("Barrel angle when charge is 0.")]
    public float minBarrelAngle = -10f;

    [Tooltip("Barrel angle when charge is 1.")]
    public float maxBarrelAngle = 25f;

    [Tooltip("Enable this if the barrel moves the wrong way.")]
    public bool invertBarrelDirection = false;

    [Header("Smoothing")]
    public float feedbackLerpSpeed = 8f;

    [Header("Debug")]
    public bool enableDebugLogs = false;

    [Range(0f, 1f)] public float targetCharge;
    [Range(0f, 1f)] public float currentCharge;

    private Quaternion initialBarrelLocalRotation;
    private float debugLogTimer;

    private void Awake()
    {
        if (fireVisual == null)
            fireVisual = transform;

        if (barrelPivot != null)
            initialBarrelLocalRotation = barrelPivot.localRotation;

        targetCharge = 0f;
        currentCharge = 0f;

        ApplyVisualsImmediate(0f);
    }

    private void Update()
    {
        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, feedbackLerpSpeed) * Time.deltaTime);
        currentCharge = Mathf.Lerp(currentCharge, targetCharge, k);
        currentCharge = Mathf.Clamp01(currentCharge);

        ApplyFireScale(currentCharge);
        ApplyBarrelRotation(currentCharge);

        if (enableDebugLogs)
        {
            debugLogTimer += Time.deltaTime;

            if (debugLogTimer >= 0.5f)
            {
                debugLogTimer = 0f;
                Debug.Log($"[CannonChargeVisualFeedback] Charge: {currentCharge:F3} | BarrelAngle: {GetMappedBarrelAngle(currentCharge):F3}");
            }
        }
    }

    public void SetChargeValue(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);
    }

    public void SetChargeImmediate(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);
        currentCharge = targetCharge;
        ApplyVisualsImmediate(currentCharge);
    }

    private void ApplyVisualsImmediate(float charge01)
    {
        ApplyFireScale(charge01);
        ApplyBarrelRotation(charge01);
    }

    private void ApplyFireScale(float charge01)
    {
        if (fireVisual == null)
            return;

        fireVisual.localScale = Vector3.Lerp(minFireScale, maxFireScale, Mathf.Clamp01(charge01));
    }

    private void ApplyBarrelRotation(float charge01)
    {
        if (barrelPivot == null)
            return;

        float angle = GetMappedBarrelAngle(charge01);

        barrelPivot.localRotation =
            initialBarrelLocalRotation *
            Quaternion.AngleAxis(angle, barrelLocalRotationAxis.normalized);
    }

    private float GetMappedBarrelAngle(float charge01)
    {
        charge01 = Mathf.Clamp01(charge01);

        if (invertBarrelDirection)
            charge01 = 1f - charge01;

        return Mathf.Lerp(minBarrelAngle, maxBarrelAngle, charge01);
    }
}