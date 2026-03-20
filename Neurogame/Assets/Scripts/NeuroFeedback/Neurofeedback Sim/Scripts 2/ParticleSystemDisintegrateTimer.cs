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
    private bool isInitialized;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void Start()
    {
        InitializeIfNeeded();

        if (playOnStart)
            StartTimer();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (duration <= 0f)
        {
            Finish();
            return;
        }

        timeRemaining -= Time.deltaTime;
        float t = Mathf.Clamp01(timeRemaining / duration);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] == null)
                continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            emission.rateOverTime = Mathf.Lerp(0f, initialEmissionRates[i], t);
            main.startSize = Mathf.Lerp(0f, initialSizes[i], t);
            main.startLifetime = Mathf.Lerp(0.05f, initialLifetimes[i], t);
        }

        if (timeRemaining <= 0f)
            Finish();
    }

    public void StartTimer()
    {
        InitializeIfNeeded();

        if (!isInitialized)
            return;

        timeRemaining = duration;
        isRunning = true;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] == null)
                continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            emission.rateOverTime = initialEmissionRates[i];
            main.startSize = initialSizes[i];
            main.startLifetime = initialLifetimes[i];

            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Clear();
            particleSystems[i].Play();
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void Finish()
    {
        isRunning = false;

        if (particleSystems != null)
        {
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (destroyWhenFinished)
        {
            if (destroyTarget != null)
                Destroy(destroyTarget);
            else
                Destroy(gameObject);
        }
    }

    private void InitializeIfNeeded()
    {
        if (isInitialized)
            return;

        if (particleSystems == null || particleSystems.Length == 0)
        {
            Debug.LogWarning("[ParticleArrayDisintegrate] No particle systems assigned.");
            return;
        }

        int count = particleSystems.Length;

        initialEmissionRates = new float[count];
        initialSizes = new float[count];
        initialLifetimes = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (particleSystems[i] == null)
                continue;

            var main = particleSystems[i].main;
            var emission = particleSystems[i].emission;

            initialEmissionRates[i] = GetConstant(emission.rateOverTime, 10f);
            initialSizes[i] = GetConstant(main.startSize, 1f);
            initialLifetimes[i] = GetConstant(main.startLifetime, 1f);
        }

        isInitialized = true;
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