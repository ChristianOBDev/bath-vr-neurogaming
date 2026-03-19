using UnityEngine;

public class ParticleArrayDisintegrate : MonoBehaviour
{
    [Header("Drag your particle systems here")]
    public ParticleSystem[] particleSystems;

    [Header("Timer")]
    public float duration = 5f;

    [Header("Options")]
    public bool playOnStart = true;
    public bool destroyWhenFinished = false;
    public GameObject destroyTarget;

    private float timeRemaining;
    private bool isRunning;

    private float[] initialEmissionRates;
    private float[] initialSizes;
    private float[] initialLifetimes;

    void Start()
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            Debug.LogError("No particle systems assigned.");
            enabled = false;
            return;
        }

        int count = particleSystems.Length;

        initialEmissionRates = new float[count];
        initialSizes = new float[count];
        initialLifetimes = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (particleSystems[i] == null) continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            initialEmissionRates[i] = GetConstant(emission.rateOverTime, 10f);
            initialSizes[i] = GetConstant(main.startSize, 1f);
            initialLifetimes[i] = GetConstant(main.startLifetime, 1f);
        }

        if (playOnStart)
            StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        float t = Mathf.Clamp01(timeRemaining / duration);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] == null) continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            emission.rateOverTime = Mathf.Lerp(0f, initialEmissionRates[i], t);
            main.startSize = Mathf.Lerp(0f, initialSizes[i], t);
            main.startLifetime = Mathf.Lerp(0.05f, initialLifetimes[i], t);
        }

        if (timeRemaining <= 0f)
        {
            Finish();
        }
    }

    public void StartTimer()
    {
        if (particleSystems == null || particleSystems.Length == 0)
            return;

        timeRemaining = duration;
        isRunning = true;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] == null) continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            // Restore original values before replaying
            emission.rateOverTime = initialEmissionRates[i];
            main.startSize = initialSizes[i];
            main.startLifetime = initialLifetimes[i];

            // Fully restart the system
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Clear();
            particleSystems[i].Play();
        }
    }

    private void Finish()
    {
        isRunning = false;

        foreach (var ps in particleSystems)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (destroyWhenFinished)
        {
            if (destroyTarget != null)
                Destroy(destroyTarget);
            else
                Destroy(gameObject);
        }
    }

    private float GetConstant(ParticleSystem.MinMaxCurve curve, float fallback)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;

            case ParticleSystemCurveMode.TwoConstants:
                return (curve.constantMin + curve.constantMax) * 0.5f;

            default:
                return fallback;
        }
    }
}