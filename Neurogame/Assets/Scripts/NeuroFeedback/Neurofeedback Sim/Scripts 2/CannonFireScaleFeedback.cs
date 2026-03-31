using UnityEngine;

public class CannonChargeVisualFeedback : MonoBehaviour
{
    [Header("Fire Visual")]
    [Tooltip("Root of the entire fire prefab.")]
    public Transform fireRoot;

    [Header("Barrel Visual")]
    [Tooltip("Assign the exact transform that should tilt up/down.")]
    public Transform barrelPivot;

    [Tooltip("Usually local X for cannon elevation. Try X, Y, or Z depending on the model setup.")]
    public Vector3 barrelLocalRotationAxis = Vector3.right;

    [Header("Spark Visual")]
    [Tooltip("Rear spark particle system. Make it a child of the barrel or of a barrel attachment point.")]
    public ParticleSystem rearSpark;

    [Header("Front Fire Timing")]
    [Tooltip("Enable this if the front fire should return to minimum before the cannon fires.")]
    public bool shrinkFrontFireBeforeShot = true;

    [Tooltip("When remaining time is less than or equal to this, the front fire starts returning to minimum.")]
    public float frontFireReturnToMinLastSeconds = 10f;

    [Header("Spark Timing")]
    [Tooltip("Only enable the rear spark during the last N seconds before firing.")]
    public float sparkActiveLastSeconds = 10f;

    [Header("Spark Intensity")]
    public float sparkMinEmission = 5f;
    public float sparkMaxEmission = 40f;
    public float sparkMinSize = 0.03f;
    public float sparkMaxSize = 0.12f;

    [Header("Fire Scale")]
    public Vector3 minFireScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 maxFireScale = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("Barrel Rotation")]
    public float minBarrelAngle = -10f;
    public float maxBarrelAngle = 25f;
    public bool invertBarrelDirection = false;

    [Header("Smoothing")]
    public float feedbackLerpSpeed = 8f;

    [Header("Debug")]
    public bool enableDebugLogs = false;
    public bool enableKeyboardTest = false;

    [Range(0f, 1f)] public float targetCharge;
    [Range(0f, 1f)] public float currentCharge;

    [SerializeField] private bool forceFrontFireToMinimum;
    [SerializeField] private float displayedFireCharge;
    [SerializeField] private Vector3 debugAppliedFireScale;
    [SerializeField] private float debugBarrelAngle;
    [SerializeField] private Vector3 debugInitialBarrelEuler;
    [SerializeField] private Vector3 debugCurrentBarrelEuler;

    private Quaternion initialBarrelLocalRotation;
    private float debugLogTimer;

    private void Awake()
    {
        if (fireRoot == null)
            fireRoot = transform;

        if (barrelPivot != null)
        {
            initialBarrelLocalRotation = barrelPivot.localRotation;
            debugInitialBarrelEuler = barrelPivot.localEulerAngles;
        }

        targetCharge = 0f;
        currentCharge = 0f;
        displayedFireCharge = 0f;
        forceFrontFireToMinimum = false;

        ApplyFireScale(0f);
        ApplyBarrelRotationImmediate(0f);
        SetSparkActive(false);

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[CannonChargeVisualFeedback] Awake | barrelPivot={(barrelPivot != null ? barrelPivot.name : "NULL")} | " +
                $"rearSpark={(rearSpark != null ? rearSpark.name : "NULL")}",
                this
            );

            if (barrelPivot != null)
            {
                Debug.Log(
                    $"[CannonChargeVisualFeedback] Awake | initial local rotation={barrelPivot.localEulerAngles} | axis={barrelLocalRotationAxis}",
                    barrelPivot
                );
            }
        }
    }

    private void Update()
    {
        HandleKeyboardTest();

        float k = 1f - Mathf.Exp(-Mathf.Max(0.01f, feedbackLerpSpeed) * Time.deltaTime);

        currentCharge = Mathf.Lerp(currentCharge, targetCharge, k);
        currentCharge = Mathf.Clamp01(currentCharge);

        float targetFireCharge = forceFrontFireToMinimum ? 0f : currentCharge;
        displayedFireCharge = Mathf.Lerp(displayedFireCharge, targetFireCharge, k);
        displayedFireCharge = Mathf.Clamp01(displayedFireCharge);

        ApplyFireScale(displayedFireCharge);

        if (enableDebugLogs)
        {
            debugLogTimer += Time.deltaTime;

            if (debugLogTimer >= 0.5f)
            {
                debugLogTimer = 0f;
                Debug.Log(
                    $"[CannonChargeVisualFeedback] " +
                    $"targetCharge={targetCharge:F3} | " +
                    $"currentCharge={currentCharge:F3} | " +
                    $"displayedFireCharge={displayedFireCharge:F3} | " +
                    $"barrelAngle={debugBarrelAngle:F3} | " +
                    $"barrelLocalEuler={(barrelPivot != null ? barrelPivot.localEulerAngles.ToString() : "NULL")}",
                    barrelPivot != null ? barrelPivot : this
                );
            }
        }
    }

    private void LateUpdate()
    {
        // Barrel rotation applied in LateUpdate so it always wins over animators
        ApplyBarrelRotationImmediate(currentCharge);
    }

    public void SetChargeValue(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);

        if (enableDebugLogs)
            Debug.Log($"[CannonChargeVisualFeedback] SetChargeValue({targetCharge:F3})", this);
    }

    public void SetChargeImmediate(float charge01)
    {
        targetCharge = Mathf.Clamp01(charge01);
        currentCharge = targetCharge;

        if (!forceFrontFireToMinimum)
            displayedFireCharge = currentCharge;

        ApplyFireScale(forceFrontFireToMinimum ? 0f : displayedFireCharge);
        ApplyBarrelRotationImmediate(currentCharge);

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[CannonChargeVisualFeedback] SetChargeImmediate({targetCharge:F3}) | " +
                $"barrelEuler={(barrelPivot != null ? barrelPivot.localEulerAngles.ToString() : "NULL")}",
                barrelPivot != null ? barrelPivot : this
            );
        }
    }

    public void UpdateSparkByTimer(float timeRemaining, float totalDuration)
    {
        if (fireRoot == null && rearSpark == null)
            return;

        forceFrontFireToMinimum =
            shrinkFrontFireBeforeShot &&
            timeRemaining <= frontFireReturnToMinLastSeconds &&
            timeRemaining > 0f;

        bool shouldBeActive = timeRemaining <= sparkActiveLastSeconds && timeRemaining > 0f;
        SetSparkActive(shouldBeActive);

        if (!shouldBeActive || rearSpark == null)
            return;

        float charge01 = Mathf.Clamp01(currentCharge);

        var emission = rearSpark.emission;
        emission.rateOverTime = Mathf.Lerp(sparkMinEmission, sparkMaxEmission, charge01);

        var main = rearSpark.main;
        main.startSize = Mathf.Lerp(sparkMinSize, sparkMaxSize, charge01);
    }

    public void ResetAllSmooth()
    {
        forceFrontFireToMinimum = false;
        SetChargeValue(0f);
        SetSparkActive(false);
    }

    public void ResetAllImmediate()
    {
        forceFrontFireToMinimum = false;
        targetCharge = 0f;
        currentCharge = 0f;
        displayedFireCharge = 0f;

        ApplyFireScale(0f);
        ApplyBarrelRotationImmediate(0f);
        SetSparkActive(false);
    }

    private void ApplyFireScale(float charge01)
    {
        if (fireRoot == null)
            return;

        Vector3 scale = Vector3.Lerp(minFireScale, maxFireScale, Mathf.Clamp01(charge01));
        fireRoot.localScale = scale;
        debugAppliedFireScale = scale;
    }

    private void ApplyBarrelRotationImmediate(float charge01)
    {
        if (barrelPivot == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[CannonChargeVisualFeedback] barrelPivot is NULL.", this);
            return;
        }

        Vector3 axis = barrelLocalRotationAxis.normalized;

        if (axis.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("[CannonChargeVisualFeedback] barrelLocalRotationAxis is zero. Rotation skipped.", this);
            return;
        }

        float angle = GetMappedBarrelAngle(charge01);
        debugBarrelAngle = angle;

        Quaternion offset = Quaternion.AngleAxis(angle, axis);
        barrelPivot.localRotation = initialBarrelLocalRotation * offset;

        debugCurrentBarrelEuler = barrelPivot.localEulerAngles;

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[CannonChargeVisualFeedback] ApplyBarrelRotation | " +
                $"charge={charge01:F3} | angle={angle:F3} | axis={axis} | " +
                $"initialEuler={debugInitialBarrelEuler} | currentEuler={debugCurrentBarrelEuler}",
                barrelPivot
            );
        }
    }

    private float GetMappedBarrelAngle(float charge01)
    {
        charge01 = Mathf.Clamp01(charge01);

        if (invertBarrelDirection)
            charge01 = 1f - charge01;

        return Mathf.Lerp(minBarrelAngle, maxBarrelAngle, charge01);
    }

    private void SetSparkActive(bool active)
    {
        if (rearSpark == null)
            return;

        if (active)
        {
            if (!rearSpark.isPlaying)
                rearSpark.Play();
        }
        else
        {
            if (rearSpark.isPlaying)
                rearSpark.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void HandleKeyboardTest()
    {
        if (!enableKeyboardTest)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetChargeImmediate(0f);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetChargeImmediate(0.5f);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetChargeImmediate(1f);
    }

    [ContextMenu("Test Barrel At 0")]
    private void TestBarrelAtZero() => SetChargeImmediate(0f);

    [ContextMenu("Test Barrel At 0.5")]
    private void TestBarrelAtHalf() => SetChargeImmediate(0.5f);

    [ContextMenu("Test Barrel At 1")]
    private void TestBarrelAtFull() => SetChargeImmediate(1f);
}