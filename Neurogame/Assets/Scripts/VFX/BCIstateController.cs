using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// BCI State Controller � lerps bird particle system parameters between
/// hardcoded relaxed and focused values based on CSV/BCI input.
///
/// SETUP:
///   1. Attach to VFXmanager.
///   2. Assign FX_Birds_Fantail_Relaxed as birdsPS (configure it in relaxed state).
///   3. Assign BoidsParticleBridge for boids control.
///   4. Drag CSV TextAssets into slots.
///   5. Focused particle system is not needed � values are hardcoded.
///
/// DEMO CONTROLS:
///   [1] Relaxed CSV  [2] Focused CSV  [3] Mixed CSV
///   [R] Force relaxed  [F] Force focused  [C] Back to CSV playback
/// </summary>
public class BCIStateController : MonoBehaviour
{
  public enum CSVSource { Relaxed, Focused, Mixed }

  [Header("CSV Playback")]
  [SerializeField] private CSVSource activeSource = CSVSource.Relaxed;
  [SerializeField] private TextAsset relaxedCSV;
  [SerializeField] private TextAsset focusedCSV;
  [SerializeField] private TextAsset mixedCSV;
  [SerializeField] private float rowInterval = 0.5f;
  [SerializeField] private bool loopCSV = true;

  private CSVSource lastSource;

  [Header("State Tuning")]
  [SerializeField] private float stateSmoothing = 2f;
  [SerializeField][Range(-0.1f, 1f)] public float manualOverride = -0.1f;

  [Header("Calibration � Log Power Ranges")]
  [SerializeField] private Vector2 frontalAlphaRange = new Vector2(-5.0f, -6.3f);
  [SerializeField] private Vector2 thetaRange = new Vector2(-5.1f, -6.8f);
  [SerializeField] private float alphaWeight = 0.5f;
  [SerializeField] private float thetaWeight = 0.5f;

  [Header("Particle Systems")]
  [SerializeField] private ParticleSystem birdsPS;
  [SerializeField] private ParticleSystem firefliesPS;

  [Header("Boids")]
  [SerializeField] private BoidsParticleBridge boidsFlock;
  [SerializeField] private float boidsScatterRelaxed = 0f;
  [SerializeField] private float boidsScatterFocused = 0.8f;

  [Header("Input Actions")]
  [SerializeField] private InputActionReference selectRelaxedCSV;
  [SerializeField] private InputActionReference selectFocusedCSV;
  [SerializeField] private InputActionReference selectMixedCSV;
  [SerializeField] private InputActionReference forceRelaxed;
  [SerializeField] private InputActionReference forceFocused;
  [SerializeField] private InputActionReference backToCSV;

  [Header("Debug")]
  [SerializeField] private bool showDebug = true;

  // CSV state
  private float rawState = 0f;
  private float smoothedState = 0f;
  private List<float[]> csvRows = new List<float[]>();
  private int currentRow = 0;
  private float rowTimer = 0f;
  private bool csvLoaded = false;

  private float frontalAlpha, frontalBeta, parietalAlpha, frontalTheta;

  public float State => smoothedState;

  public void SetBandPowers(float fAlpha, float fBeta, float pAlpha, float fTheta)
  {
    frontalAlpha = fAlpha;
    frontalBeta = fBeta;
    parietalAlpha = pAlpha;
    frontalTheta = fTheta;
    rawState = ComputeState();
  }

  private void OnEnable()
  {
        if (selectRelaxedCSV != null)
        {
            selectRelaxedCSV.action.performed += OnSelectRelaxedCSV;
            selectRelaxedCSV.action.Enable();
        }
        if (selectFocusedCSV != null)
        {
            selectFocusedCSV.action.performed += OnSelectFocusedCSV;
            selectFocusedCSV.action.Enable();
        }
        if (selectMixedCSV != null)
        {
            selectMixedCSV.action.performed += OnSelectMixedCSV;
            selectMixedCSV.action.Enable();
        }
        if (forceRelaxed != null)
        {
            forceRelaxed.action.performed += OnForceRelaxed;
            forceRelaxed.action.Enable();
        }
        if (forceFocused != null)
        {
            forceFocused.action.performed += OnForceFocused;
            forceFocused.action.Enable();
        }
        if (backToCSV != null)
        {
            backToCSV.action.performed += OnBackToCSV;
            backToCSV.action.Enable();
        }
    }

  private void OnDisable()
  {
        if (selectRelaxedCSV != null)
        {
            selectRelaxedCSV.action.performed -= OnSelectRelaxedCSV;
            selectRelaxedCSV.action.Disable();
        }
        if (selectFocusedCSV != null)
        {
            selectFocusedCSV.action.performed -= OnSelectFocusedCSV;
            selectFocusedCSV.action.Disable();
        }
        if (selectMixedCSV != null)
        {
            selectMixedCSV.action.performed -= OnSelectMixedCSV;
            selectMixedCSV.action.Disable();
        }
        if (forceRelaxed != null)
        {
            forceRelaxed.action.performed -= OnForceRelaxed;
            forceRelaxed.action.Disable();
        }
        if (forceFocused != null)
        {
            forceFocused.action.performed -= OnForceFocused;
            forceFocused.action.Disable();
        }
        if (backToCSV != null)
        {
            backToCSV.action.performed -= OnBackToCSV;
            backToCSV.action.Disable();
        }
    }

  private void Start()
  {
    lastSource = activeSource;
    LoadCSV(GetActiveCSV());
  }

  private void Update()
  {
    if (activeSource != lastSource)
    {
      lastSource = activeSource;
      LoadCSV(GetActiveCSV());
    }

    if (csvLoaded && manualOverride < 0f)
    {
      rowTimer += Time.deltaTime;
      if (rowTimer >= rowInterval)
      {
        rowTimer = 0f;
        ReadNextRow();
      }
    }

    if (manualOverride >= 0f)
      rawState = manualOverride;

    smoothedState = Mathf.MoveTowards(smoothedState, rawState, Time.deltaTime * stateSmoothing);
    smoothedState = Mathf.Clamp01(smoothedState);

    ApplyBirds();
    ApplyFireflies();
    ApplyBoids();
  }

  // =========================================================================
  // Birds � hardcoded relaxed/focused values, lerped by smoothedState
  // =========================================================================
  private void ApplyBirds()
  {
    if (birdsPS == null) return;

    float t = smoothedState;
    var main = birdsPS.main;

    // --- Main Module ---
    // startSpeed: relaxed 0, focused 6
    var speed = main.startSpeed;
    speed.constantMax = Mathf.Lerp(0f, 6f, t);
    main.startSpeed = speed;

    // --- Emission ---
    // rateOverTime: relaxed 2, focused 50
    var emission = birdsPS.emission;
    emission.rateOverTime = Mathf.Lerp(2f, 50f, t);

    // --- Shape ---
    var shape = birdsPS.shape;
    // radius: relaxed 13.19, focused 6
    shape.radius = Mathf.Lerp(13.19f, 6f, t);
    // radiusThickness: relaxed 1, focused 0.6
    shape.radiusThickness = Mathf.Lerp(1f, 0.6f, t);
    // arc spread: relaxed 0, focused 1
    // Note: arc spread isn't directly accessible, using arc mode spread
    // shape.arcSpread is not a direct API � we use shape.arcSpeedMultiplier or set via arc
    // Actually spread is: shape.arc = degrees, arcSpread is a separate float
#if UNITY_2021_1_OR_NEWER
    // Arc spread doesn't have a direct setter in all versions.
    // If this causes a compile error, comment it out.
#endif
    // m_Scale.z: relaxed 0.5, focused 2
    Vector3 shapeScale = shape.scale;
    shapeScale.z = Mathf.Lerp(0.5f, 2f, t);
    shape.scale = shapeScale;
    // randomDirectionAmount: relaxed 0, focused 0.3
    shape.randomDirectionAmount = Mathf.Lerp(0f, 0.3f, t);

    // --- Velocity over Lifetime ---
    var vel = birdsPS.velocityOverLifetime;
    vel.enabled = true;

    // Linear XYZ: relaxed (0,0), focused (0,1) � random between two constants
    var vx = vel.x;
    vx.constantMin = 0f;
    vx.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.x = vx;

    var vy = vel.y;
    vy.constantMin = 0f;
    vy.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.y = vy;

    var vz = vel.z;
    vz.constantMin = 0f;
    vz.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.z = vz;

    // Orbital XYZ: relaxed (0,0), focused (0,1)
    var ox = vel.orbitalX;
    ox.constantMin = 0f;
    ox.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.orbitalX = ox;

    var oy = vel.orbitalY;
    oy.constantMin = 0f;
    oy.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.orbitalY = oy;

    var oz = vel.orbitalZ;
    oz.constantMin = 0f;
    oz.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.orbitalZ = oz;

    // Orbital Offset XYZ: relaxed (0,0), focused (-0.3, 0.3)
    var offx = vel.orbitalOffsetX;
    offx.constantMin = Mathf.Lerp(0f, -0.3f, t);
    offx.constantMax = Mathf.Lerp(0f, 0.3f, t);
    vel.orbitalOffsetX = offx;

    var offy = vel.orbitalOffsetY;
    offy.constantMin = Mathf.Lerp(0f, -0.3f, t);
    offy.constantMax = Mathf.Lerp(0f, 0.3f, t);
    vel.orbitalOffsetY = offy;

    var offz = vel.orbitalOffsetZ;
    offz.constantMin = Mathf.Lerp(0f, -0.3f, t);
    offz.constantMax = Mathf.Lerp(0f, 0.3f, t);
    vel.orbitalOffsetZ = offz;

    // Radial: relaxed (0,0), focused (0,1)
    var rad = vel.radial;
    rad.constantMin = 0f;
    rad.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.radial = rad;

    // --- Limit Velocity over Lifetime ---
    var limit = birdsPS.limitVelocityOverLifetime;
    limit.enabled = true;
    // Speed: relaxed 1, focused 3
    limit.limit = Mathf.Lerp(1f, 3f, t);
    // Drag: relaxed 0.4, focused 0.1
    limit.drag = Mathf.Lerp(0.4f, 0.1f, t);

    // --- Force over Lifetime ---
    var force = birdsPS.forceOverLifetime;
    force.enabled = true;

    // X: relaxed (0,0), focused (2, -6)
    var fx = force.x;
    fx.constantMin = Mathf.Lerp(0f, -6f, t);
    fx.constantMax = Mathf.Lerp(0f, 2f, t);
    force.x = fx;

    // Y: relaxed (0,0), focused (2, -5.05)
    var fy = force.y;
    fy.constantMin = Mathf.Lerp(0f, -5.05f, t);
    fy.constantMax = Mathf.Lerp(0f, 2f, t);
    force.y = fy;

    // Z: relaxed (0,0), focused (2, -6)
    var fz = force.z;
    fz.constantMin = Mathf.Lerp(0f, -6f, t);
    fz.constantMax = Mathf.Lerp(0f, 2f, t);
    force.z = fz;

    // --- External Forces ---
    var extForces = birdsPS.externalForces;
    extForces.enabled = true;
    extForces.multiplier = Mathf.Lerp(0f, 10f, t);

    // --- Noise ---
    var noise = birdsPS.noise;
    noise.enabled = true;

    // Strength X: relaxed 4, focused 8
    var nsx = noise.strengthX;
    nsx.constantMin = Mathf.Lerp(2f, 12f, t);
    nsx.constantMax = Mathf.Lerp(4f, 8f, t);
    noise.strengthX = nsx;

    // Strength Y: relaxed (2, 0.2), focused (5, 8)
    var nsy = noise.strengthY;
    nsy.constantMin = Mathf.Lerp(0.2f, 8f, t);
    nsy.constantMax = Mathf.Lerp(2f, 5f, t);
    noise.strengthY = nsy;

    // Strength Z: relaxed (4, 2), focused (8, 12)
    var nsz = noise.strengthZ;
    nsz.constantMin = Mathf.Lerp(2f, 12f, t);
    nsz.constantMax = Mathf.Lerp(4f, 8f, t);
    noise.strengthZ = nsz;

    // scrollSpeed: relaxed 1, focused 0.75
    var ss = noise.scrollSpeed;
    ss.constantMax = Mathf.Lerp(1f, 0.75f, t);
    noise.scrollSpeed = ss;

    // sizeAmount: relaxed 0, focused 0.1
    noise.sizeAmount = Mathf.Lerp(0f, 0.1f, t);
  }

  // =========================================================================
  // Fireflies � hardcoded relaxed/focused values
  // =========================================================================
  private void ApplyFireflies()
  {
    if (firefliesPS == null) return;

    float t = smoothedState;
    var main = firefliesPS.main;

    // --- Main Module ---
    // startSpeed: relaxed 0, focused 0.5
    var speed = main.startSpeed;
    speed.constantMax = Mathf.Lerp(0f, 0.5f, t);
    main.startSpeed = speed;

    // --- Emission ---
    // rateOverTime: relaxed 10, focused 30
    var emission = firefliesPS.emission;
    emission.rateOverTime = Mathf.Lerp(10f, 30f, t);

    // --- Shape ---
    var shape = firefliesPS.shape;
    // radius: relaxed 9.21, focused 2.99
    shape.radius = Mathf.Lerp(9.21f, 2.99f, t);

    // --- Velocity over Lifetime ---
    var vel = firefliesPS.velocityOverLifetime;
    vel.enabled = true;

    // Orbital X: relaxed 0, focused 0.5
    var ox = vel.orbitalX;
    ox.constantMax = Mathf.Lerp(0f, 0.5f, t);
    vel.orbitalX = ox;

    // Orbital Y: relaxed 0, focused 0.5
    var oy = vel.orbitalY;
    oy.constantMax = Mathf.Lerp(0f, 0.5f, t);
    vel.orbitalY = oy;

    // Orbital Z: relaxed 0, focused 1
    var oz = vel.orbitalZ;
    oz.constantMax = Mathf.Lerp(0f, 1f, t);
    vel.orbitalZ = oz;

    // Radial: relaxed 0, focused -0.05
    var rad = vel.radial;
    rad.constantMax = Mathf.Lerp(0f, -0.05f, t);
    vel.radial = rad;

    // Speed modifier: relaxed (1,1), focused (1,3)
    var sm = vel.speedModifier;
    sm.constantMin = 1f;
    sm.constantMax = Mathf.Lerp(1f, 3f, t);
    vel.speedModifier = sm;

    // --- External Forces ---
    var extForces = firefliesPS.externalForces;
    extForces.enabled = true;
    extForces.multiplier = Mathf.Lerp(0f, 1f, t);

    // --- Noise ---
    var noise = firefliesPS.noise;
    if (noise.enabled)
    {
      noise.sizeAmount = Mathf.Lerp(0f, 0.2f, t);
    }
  }

  // =========================================================================
  // Boids
  // =========================================================================
  private void ApplyBoids()
  {
    if (boidsFlock == null) return;

    float t = smoothedState;
    boidsFlock.SetScatter(Mathf.Lerp(boidsScatterRelaxed, boidsScatterFocused, t));
    boidsFlock.SetSpeed(Mathf.Lerp(1f, 2f, t));
    boidsFlock.SetMaxSpeed(Mathf.Lerp(4f, 6f, t));
    boidsFlock.SetCohesionWeight(Mathf.Lerp(6f, 3f, t));
  }

  // =========================================================================
  // Input Action Callbacks
  // =========================================================================
  private void OnSelectRelaxedCSV(InputAction.CallbackContext context)
  {
    activeSource = CSVSource.Relaxed;
    manualOverride = -0.1f;
    if (showDebug) Debug.Log("[BCIState] Source: RELAXED CSV");
  }

  private void OnSelectFocusedCSV(InputAction.CallbackContext context)
  {
    activeSource = CSVSource.Focused;
    manualOverride = -0.1f;
    if (showDebug) Debug.Log("[BCIState] Source: FOCUSED CSV");
  }

  private void OnSelectMixedCSV(InputAction.CallbackContext context)
  {
    activeSource = CSVSource.Mixed;
    manualOverride = -0.1f;
    if (showDebug) Debug.Log("[BCIState] Source: MIXED CSV");
  }

  private void OnForceRelaxed(InputAction.CallbackContext context)
  {
    manualOverride = 0f;
    if (showDebug) Debug.Log("[BCIState] Manual override: RELAXED");
  }

  private void OnForceFocused(InputAction.CallbackContext context)
  {
    manualOverride = 1f;
    if (showDebug) Debug.Log("[BCIState] Manual override: FOCUSED");
  }

  private void OnBackToCSV(InputAction.CallbackContext context)
  {
    manualOverride = -0.1f;
    if (showDebug) Debug.Log("[BCIState] Back to CSV playback");
  }

  private TextAsset GetActiveCSV()
  {
    switch (activeSource)
    {
      case CSVSource.Relaxed: return relaxedCSV;
      case CSVSource.Focused: return focusedCSV;
      case CSVSource.Mixed: return mixedCSV;
      default: return relaxedCSV;
    }
  }

  private void LoadCSV(TextAsset csv)
  {
    csvRows.Clear();
    currentRow = 0;
    rowTimer = 0f;
    csvLoaded = false;

    if (csv == null)
    {
      Debug.LogWarning($"[BCIState] No CSV assigned for {activeSource}.");
      return;
    }

    string[] lines = csv.text.Split('\n');

    for (int i = 0; i < lines.Length; i++)
    {
      string line = lines[i].Trim();
      if (string.IsNullOrEmpty(line)) continue;

      string[] cols = line.Split(',');
      if (cols.Length < 4) continue;

      float[] values = new float[4];
      bool valid = true;
      for (int c = 0; c < 4; c++)
      {
        if (!float.TryParse(cols[c].Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out values[c]))
        {
          valid = false;
          break;
        }
      }

      if (valid)
        csvRows.Add(values);
    }

    csvLoaded = csvRows.Count > 0;
    if (showDebug)
      Debug.Log($"[BCIState] Loaded {csvRows.Count} rows from {csv.name}");
  }

  private void ReadNextRow()
  {
    if (csvRows.Count == 0) return;

    float[] row = csvRows[currentRow];

    frontalAlpha = row[0];
    frontalBeta = row[1];
    parietalAlpha = row[2];
    frontalTheta = row[3];

    rawState = ComputeState();

    currentRow++;
    if (currentRow >= csvRows.Count)
    {
      if (loopCSV)
        currentRow = 0;
      else
        currentRow = csvRows.Count - 1;
    }

    if (showDebug && currentRow % 10 == 0)
      Debug.Log($"[BCIState] Row {currentRow}/{csvRows.Count} | " +
                $"fAlpha={frontalAlpha:F3} theta={frontalTheta:F3} | " +
                $"raw={rawState:F3} smooth={smoothedState:F3}");
  }

  private float ComputeState()
  {
    float alphaNorm = Mathf.InverseLerp(frontalAlphaRange.x, frontalAlphaRange.y, frontalAlpha);
    float thetaNorm = Mathf.InverseLerp(thetaRange.x, thetaRange.y, frontalTheta);
    float combined = (alphaNorm * alphaWeight + thetaNorm * thetaWeight) / (alphaWeight + thetaWeight);
    return Mathf.Clamp01(combined);
  }

  // =========================================================================
  // Debug GUI
  // =========================================================================
  private void OnGUI()
  {
    if (!showDebug) return;

    GUILayout.BeginArea(new Rect(10, 10, 400, 220));
    GUILayout.Box($"BCI State: {smoothedState:F3}  ({(smoothedState < 0.4f ? "RELAXED" : smoothedState > 0.6f ? "FOCUSED" : "TRANSITION")})");
    GUILayout.Box($"Raw: {rawState:F3} | Row: {currentRow}/{csvRows.Count}");
    GUILayout.Box($"fAlpha: {frontalAlpha:F3} | fBeta: {frontalBeta:F3}");
    GUILayout.Box($"pAlpha: {parietalAlpha:F3} | theta: {frontalTheta:F3}");
    GUILayout.Box($"[R] Relaxed  [F] Focused  [C] CSV  |  Override: {(manualOverride >= 0f ? manualOverride.ToString("F1") : "OFF")}");
    GUILayout.Box($"[1] Relaxed CSV  [2] Focused CSV  [3] Mixed CSV  |  Active: {activeSource}");
    GUILayout.EndArea();
  }
}