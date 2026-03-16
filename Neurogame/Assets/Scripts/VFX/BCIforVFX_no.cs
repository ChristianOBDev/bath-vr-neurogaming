using UnityEngine;
using System.Collections.Generic;

public class BCIforVFX : MonoBehaviour
{
    [Header("CSV Data")]
    [Tooltip("CSV for relaxed state (high fluidness)")]
    [SerializeField] private TextAsset relaxedCSV;

    [Tooltip("CSV for focused state (low fluidness)")]
    [SerializeField] private TextAsset focusedCSV;

    [Header("State Switch (for testing / tuning)")]
    [Tooltip("Force relaxed state (overrides real BCI)")]
    public bool forceRelaxed = false;

    [Tooltip("Force focused state (overrides real BCI)")]
    public bool forceFocused = false;

    [Tooltip("Blend between focused (0) and relaxed (1) for transition testing")]
    [Range(0f, 1f)]
    public float manualFluidnessBlend = 0.5f;

    [Tooltip("Use manual blend instead of real BCI data?")]
    public bool useManualBlend = true;

    [Header("Playback")]
    [Tooltip("Seconds between CSV row updates")]
    public float timePerRow = 5f;

    [Header("Synty Particle Systems")]
    public ParticleSystem birdsParticles;
    public ParticleSystem dustParticles;
    public ParticleSystem firefliesParticles;

    [Header("Forces & Mappings (tuning)")]
    public float maxCohesion = 2f;
    public float maxAlignment = 1.5f;
    public float separationForce = 1.2f;
    public float maxReturnForce = 0.8f;
    public float maxNoise = 3f;
    public float maxVortex = 2.5f;

    public Vector2 spreadRange = new Vector2(0.5f, 6f);
    public Vector2 speedRange = new Vector2(2f, 12f);
    public Vector2 brightnessRange = new Vector2(0.4f, 1.8f);
    public Vector2 trailRange = new Vector2(0.2f, 3f);

    private List<float[]> relaxedData = new List<float[]>();
    private List<float[]> focusedData = new List<float[]>();
    private float timer = 0f;
    private int currentRelaxed = 0;
    private int currentFocused = 0;

    private float currentFluidness = 0.5f;
    private float frontalAlphaNorm = 0f;
    private float frontalBetaNorm = 0f;
    private float parietalAlphaNorm = 0f;
    private float frontalMidThetaNorm = 0f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadCSV(relaxedCSV, relaxedData);
        LoadCSV(focusedCSV, focusedData);

        if (relaxedData.Count == 0 || focusedData.Count == 0)
        {
            Debug.LogError("CSV load failed!");
            return;
        }

        Debug.Log($"BCIforVFX: Loaded {relaxedData.Count} relaxed + {focusedData.Count} focused rows");

        UpdateState(); // initial
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timePerRow)
        {
            timer -= timePerRow;
            currentRelaxed = (currentRelaxed + 1) % relaxedData.Count;
            currentFocused = (currentFocused + 1) % focusedData.Count;
            UpdateState();
        }
    }

    private void LoadCSV(TextAsset csv, List<float[]> target)
    {
        if (csv == null || string.IsNullOrEmpty(csv.text)) return;

        string[] lines = csv.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length < 4) continue;

            float[] row = new float[4];
            bool valid = true;
            for (int i = 0; i < 4; i++)
            {
                if (!float.TryParse(parts[i].Trim(), out row[i]))
                {
                    valid = false;
                    break;
                }
            }
            if (valid) target.Add(row);
        }
    }

    private void UpdateState()
    {
        float[] relaxed = relaxedData[currentRelaxed];
        float[] focused = focusedData[currentFocused];

        // Fluidness control
        if (forceRelaxed)
        {
            currentFluidness = 1f;
        }
        else if (forceFocused)
        {
            currentFluidness = 0f;
        }
        else if (useManualBlend)
        {
            currentFluidness = manualFluidnessBlend;  // ← slider for testing
        }
        else
        {
            // Real BCI blend (your original formula)
            float relaxedFluid = relaxed[0] * 0.4f + relaxed[2] * 0.3f + relaxed[3] * 0.3f - relaxed[1] * 0.4f;
            float focusedFluid = focused[0] * 0.4f + focused[2] * 0.3f + focused[3] * 0.3f - focused[1] * 0.4f;
            currentFluidness = Mathf.Lerp(focusedFluid, relaxedFluid, 0.5f);
            currentFluidness = Mathf.Clamp01(currentFluidness);
        }

        // Secondary norms (blend based on fluidness)
        frontalAlphaNorm = Mathf.Lerp(focused[0], relaxed[0], currentFluidness);
        frontalBetaNorm = Mathf.Lerp(focused[1], relaxed[1], currentFluidness);
        parietalAlphaNorm = Mathf.Lerp(focused[2], relaxed[2], currentFluidness);
        frontalMidThetaNorm = Mathf.Lerp(focused[3], relaxed[3], currentFluidness);

        // Apply to particles
        ApplyToParticles(birdsParticles);
        ApplyToParticles(dustParticles);
        ApplyToParticles(firefliesParticles);

        Debug.Log($"BCIforVFX: fluidness = {currentFluidness:F2} (override: {useManualBlend}, relaxed: {forceRelaxed}, focused: {forceFocused})");
    }

    private void ApplyToParticles(ParticleSystem ps)
    {
        if (ps == null) return;

        var main = ps.main;
        var shape = ps.shape;
        var force = ps.forceOverLifetime;
        var noise = ps.noise;

        // Secondary mappings
        float spread = Mathf.Lerp(spreadRange.x, spreadRange.y, frontalAlphaNorm);
        float speed = Mathf.Lerp(speedRange.x, speedRange.y, frontalBetaNorm);
        float brightness = Mathf.Lerp(brightnessRange.x, brightnessRange.y, parietalAlphaNorm);
        float trailTime = Mathf.Lerp(trailRange.x, trailRange.y, frontalMidThetaNorm);

        main.startSpeed = speed;
        main.startLifetime = trailTime;  // controls trail length with Die with Particles
        shape.radius = spread;

        // Color
        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(new Color(brightness, brightness, brightness));

        // Forces & noise
        force.enabled = true;
        force.space = ParticleSystemSimulationSpace.World;

        float cohesion = Mathf.Lerp(0f, maxCohesion, 1f - currentFluidness);
        float alignment = Mathf.Lerp(0f, maxAlignment, 1f - currentFluidness);
        float returnStrength = Mathf.Lerp(0f, maxReturnForce, 1f - currentFluidness);

        force.x = new ParticleSystem.MinMaxCurve(cohesion);
        force.y = new ParticleSystem.MinMaxCurve(alignment);
        force.z = new ParticleSystem.MinMaxCurve(returnStrength);

        float vortex = maxVortex * (1f - Mathf.Abs(currentFluidness - 0.5f) / 0.2f);
        vortex = Mathf.Max(0f, vortex);
        force.yMultiplier = vortex;

        noise.enabled = true;
        noise.strength = Mathf.Lerp(0f, maxNoise, currentFluidness);
        noise.frequency = Mathf.Lerp(0.2f, 1.5f, currentFluidness);
        noise.scrollSpeed = currentFluidness * 3f;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }
}