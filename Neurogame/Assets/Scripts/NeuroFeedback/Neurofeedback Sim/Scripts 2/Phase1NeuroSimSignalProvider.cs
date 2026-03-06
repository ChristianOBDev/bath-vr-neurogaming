using UnityEngine;

public class SlowGimmickSignalProvider : MonoBehaviour, ISignalProvider, IResettableSignal
{
    [Header("Output")]
    [Range(0f, 1f)] public float signal = 0f;

    [Header("Behaviour")]
    [Tooltip("How fast the signal rises per second when 'helping'.")]
    public float risePerSecond = 0.18f;

    [Tooltip("How fast the signal falls per second when not 'helping'. Set 0 to never fall.")]
    public float fallPerSecond = 0.10f;

    [Tooltip("Extra smoothing (bigger = smoother/slower response).")]
    public float smoothing = 6f;

    [Header("Guidance")]
    [Tooltip("In Phase 1, we can gently guide toward this level so it looks gradual.")]
    [Range(0f, 1f)] public float guidedTargetLevel = 0.55f;

    [Tooltip("How strongly it drifts toward guidedTargetLevel (0 = off).")]
    public float guidedDriftStrength = 0.20f;

    float _display;

    public void ResetSignal()
    {
        signal = 0f;
        _display = 0f;
    }

    private void OnEnable()
    {
        ResetSignal();
    }

    /// <summary>
    /// Call this each frame with whether we want it to rise or fall.
    /// </summary>
    public void Tick(bool shouldRise)
    {
        float dt = Time.deltaTime;

        float target = signal;

        if (shouldRise) target += risePerSecond * dt;
        else target -= fallPerSecond * dt;

        // Optional: gentle drift toward a guided level (Phase1 “auto” feel)
        if (guidedDriftStrength > 0f)
            target = Mathf.Lerp(target, guidedTargetLevel, guidedDriftStrength * dt);

        target = Mathf.Clamp01(target);

        // Smooth the displayed output so it moves gradually
        float k = 1f - Mathf.Exp(-smoothing * dt);
        _display = Mathf.Lerp(_display, target, k);

        signal = _display;
    }

    public float GetSignal01() => signal;
}