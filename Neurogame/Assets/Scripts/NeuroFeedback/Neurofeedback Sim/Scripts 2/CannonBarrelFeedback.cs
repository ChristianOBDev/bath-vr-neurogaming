using UnityEngine;

public class CannonBarrelFeedback : MonoBehaviour
{
    [Header("Barrel Transform")]
    [Tooltip("Transform that tilts up/down.")]
    public Transform barrelPivot;

    [Header("Rotation Axis")]
    [Tooltip("Usually X for cannon elevation.")]
    public Vector3 localRotationAxis = Vector3.right;

    [Header("Angle Range")]
    [Tooltip("Angle when charge is 0.")]
    public float minAngle = -5f;

    [Tooltip("Angle when charge is 1.")]
    public float maxAngle = 20f;

    [Header("Direction")]
    [Tooltip("Enable this if the barrel moves the wrong way.")]
    public bool invertDirection = false;

    [Header("Smoothing")]
    public float rotationLerpSpeed = 8f;

    private Quaternion initialLocalRotation;
    private float targetCharge;
    private float currentAngle;

    private void Awake()
    {
        if (barrelPivot == null)
            barrelPivot = transform;

        initialLocalRotation = barrelPivot.localRotation;
        targetCharge = 0f;
        currentAngle = GetMappedAngle(0f);
        ApplyAngle(currentAngle);
    }

    private void Update()
    {
        float targetAngle = GetMappedAngle(targetCharge);

        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationLerpSpeed) * Time.deltaTime);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, k);

        ApplyAngle(currentAngle);
    }

    public void SetChargeValue(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);
    }

    public void SetChargeImmediate(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);
        currentAngle = GetMappedAngle(targetCharge);
        ApplyAngle(currentAngle);
    }

    private float GetMappedAngle(float charge01)
    {
        charge01 = Mathf.Clamp01(charge01);

        if (invertDirection)
            charge01 = 1f - charge01;

        return Mathf.Lerp(minAngle, maxAngle, charge01);
    }

    private void ApplyAngle(float angle)
    {
        if (barrelPivot == null)
            return;

        barrelPivot.localRotation =
            initialLocalRotation *
            Quaternion.AngleAxis(angle, localRotationAxis.normalized);
    }
}