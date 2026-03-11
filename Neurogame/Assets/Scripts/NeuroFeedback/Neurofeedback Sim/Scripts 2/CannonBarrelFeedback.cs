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
    public bool invertDirection = true;

    [Header("Smoothing")]
    public float rotationLerpSpeed = 8f;

    private Quaternion _initialLocalRotation;
    private float _targetCharge;
    private float _currentAngle;

    private void Awake()
    {
        if (barrelPivot == null)
            barrelPivot = transform;

        _initialLocalRotation = barrelPivot.localRotation;
        _targetCharge = 0f;
        _currentAngle = GetMappedAngle(0f);
        ApplyAngle(_currentAngle);
    }

    private void Update()
    {
        float targetAngle = GetMappedAngle(_targetCharge);

        float k = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);
        _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, k);

        ApplyAngle(_currentAngle);
    }

    public void SetChargeValue(float charge01)
    {
        _targetCharge = Mathf.Clamp01(charge01);
    }

    public void SetChargeImmediate(float charge01)
    {
        _targetCharge = Mathf.Clamp01(charge01);
        _currentAngle = GetMappedAngle(_targetCharge);
        ApplyAngle(_currentAngle);
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
        barrelPivot.localRotation =
            _initialLocalRotation *
            Quaternion.AngleAxis(angle, localRotationAxis.normalized);
    }
}