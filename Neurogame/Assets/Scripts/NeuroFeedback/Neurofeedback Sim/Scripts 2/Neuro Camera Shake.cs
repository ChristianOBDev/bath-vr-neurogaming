using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NeuroCameraShakeOnEnable : MonoBehaviour
{
    public enum ShakeSpace { Screen, World }

    [Header("Cameras")]
    public bool useMainCamera = true;
    public List<Camera> cameras = new List<Camera>();

    [Header("Timing")]
    public float delay = 0f;
    public float duration = 0.35f;

    [Header("Shake")]
    public ShakeSpace shakeSpace = ShakeSpace.Screen;
    public Vector3 shakeStrength = new Vector3(0.25f, 0.25f, 0.15f);
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Tooltip("If > 0, camera position will only update every X seconds (choppier but sometimes nicer).")]
    [Range(0, 0.1f)] public float shakesDelay = 0f;

    [Header("Optional")]
    [Tooltip("If assigned, we can stop shaking when this particle system is no longer alive.")]
    public ParticleSystem stopWhenParticleStops;

    [Tooltip("Multiply shake by distance falloff from this effect to the camera. 0 = no falloff.")]
    public float distanceFalloff = 0f;

    [Tooltip("If distanceFalloff > 0, shake is zero at this distance and beyond.")]
    public float maxDistance = 25f;

    // runtime
    public bool IsShaking { get; private set; }
    float _time;
    float _delaysTimer;
    Vector3 _shakeVector;

    readonly Dictionary<Camera, Vector3> _preRenderLocalPos = new Dictionary<Camera, Vector3>();

    // ---- Static dispatcher (mirrors CartoonFX approach) ----
    static bool s_CallbackRegistered;
    static readonly List<NeuroCameraShakeOnEnable> s_ActiveShakes = new List<NeuroCameraShakeOnEnable>();

#if UNITY_2019_1_OR_NEWER
    static void BeginCameraRendering_URP(ScriptableRenderContext context, Camera cam) => OnPreRender_Static(cam);
    static void EndCameraRendering_URP(ScriptableRenderContext context, Camera cam) => OnPostRender_Static(cam);
#endif

    static void OnPreRender_Static(Camera cam)
    {
        for (int i = 0; i < s_ActiveShakes.Count; i++)
            s_ActiveShakes[i].OnPreRenderCamera(cam);
    }

    static void OnPostRender_Static(Camera cam)
    {
        for (int i = s_ActiveShakes.Count - 1; i >= 0; i--)
            s_ActiveShakes[i].OnPostRenderCamera(cam);
    }

    static void RegisterCallbacksIfNeeded()
    {
        if (s_CallbackRegistered) return;

#if UNITY_2019_1_OR_NEWER
#if UNITY_2019_3_OR_NEWER
        bool builtIn = GraphicsSettings.currentRenderPipeline == null;
#else
        bool builtIn = GraphicsSettings.renderPipelineAsset == null;
#endif
        if (builtIn)
        {
            Camera.onPreRender += OnPreRender_Static;
            Camera.onPostRender += OnPostRender_Static;
        }
        else
        {
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering_URP;
            RenderPipelineManager.endCameraRendering += EndCameraRendering_URP;
        }
#else
        Camera.onPreRender += OnPreRender_Static;
        Camera.onPostRender += OnPostRender_Static;
#endif

        s_CallbackRegistered = true;
    }

    static void UnregisterCallbacksIfPossible()
    {
        if (!s_CallbackRegistered) return;
        if (s_ActiveShakes.Count > 0) return;

#if UNITY_2019_1_OR_NEWER
#if UNITY_2019_3_OR_NEWER
        bool builtIn = GraphicsSettings.currentRenderPipeline == null;
#else
        bool builtIn = GraphicsSettings.renderPipelineAsset == null;
#endif
        if (builtIn)
        {
            Camera.onPreRender -= OnPreRender_Static;
            Camera.onPostRender -= OnPostRender_Static;
        }
        else
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering_URP;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering_URP;
        }
#else
        Camera.onPreRender -= OnPreRender_Static;
        Camera.onPostRender -= OnPostRender_Static;
#endif

        s_CallbackRegistered = false;
    }

    // ---- Unity lifecycle ----

    void OnEnable()
    {
        FetchCameras();
        StartShake();
    }

    void OnDisable()
    {
        StopShake();
    }

    void Update()
    {
        if (!IsShaking) return;

        // If bound to a particle system, stop when it dies.
        if (stopWhenParticleStops != null && !stopWhenParticleStops.IsAlive(true))
        {
            StopShake();
            return;
        }

        _time += Time.deltaTime;

        float total = delay + duration;
        if (_time < delay) return;

        if (_time >= total)
        {
            StopShake();
            return;
        }

        float delta01 = Mathf.Clamp01((_time - delay) / Mathf.Max(0.0001f, duration));

        if (shakesDelay > 0f)
        {
            _delaysTimer += Time.deltaTime;
            if (_delaysTimer < shakesDelay) return;
            while (_delaysTimer >= shakesDelay) _delaysTimer -= shakesDelay;
        }

        var rand = new Vector3(Random.value, Random.value, Random.value);
        var shakeVec = Vector3.Scale(rand, shakeStrength) * (Random.value > 0.5f ? -1 : 1);

        float curve = shakeCurve.Evaluate(delta01);
        float falloff = ComputeDistanceFalloff();

        _shakeVector = shakeVec * curve * falloff;
    }

    // ---- Public control ----

    public void FetchCameras()
    {
        // Remove nulls
        cameras.RemoveAll(c => c == null);

        if (useMainCamera && Camera.main != null && !cameras.Contains(Camera.main))
            cameras.Add(Camera.main);

        foreach (var cam in cameras)
        {
            if (cam == null) continue;
            if (!_preRenderLocalPos.ContainsKey(cam))
                _preRenderLocalPos.Add(cam, cam.transform.localPosition);
        }
    }

    public void StartShake()
    {
        _time = 0f;
        _delaysTimer = 0f;
        _shakeVector = Vector3.zero;

        if (!s_ActiveShakes.Contains(this))
            s_ActiveShakes.Add(this);

        RegisterCallbacksIfNeeded();
        IsShaking = true;
    }

    public void StopShake()
    {
        IsShaking = false;
        _shakeVector = Vector3.zero;

        s_ActiveShakes.Remove(this);

        // restore any tracked cameras just in case
        foreach (var kvp in _preRenderLocalPos)
        {
            if (kvp.Key != null)
                kvp.Key.transform.localPosition = kvp.Value;
        }

        UnregisterCallbacksIfPossible();
    }

    // ---- Camera callbacks ----

    void OnPreRenderCamera(Camera cam)
    {
        if (!IsShaking) return;
        if (!_preRenderLocalPos.ContainsKey(cam)) return;
        if (Time.timeScale <= 0f) return;

        _preRenderLocalPos[cam] = cam.transform.localPosition;

        switch (shakeSpace)
        {
            case ShakeSpace.Screen:
                cam.transform.localPosition += cam.transform.rotation * _shakeVector;
                break;
            case ShakeSpace.World:
                cam.transform.localPosition += _shakeVector;
                break;
        }
    }

    void OnPostRenderCamera(Camera cam)
    {
        if (_preRenderLocalPos.TryGetValue(cam, out var pos))
            cam.transform.localPosition = pos;
    }

    float ComputeDistanceFalloff()
    {
        if (distanceFalloff <= 0f) return 1f;
        if (cameras == null || cameras.Count == 0) return 1f;

        // Use closest camera distance
        float best = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var cam in cameras)
        {
            if (cam == null) continue;
            float d = Vector3.Distance(cam.transform.position, p);
            if (d < best) best = d;
        }

        if (best == float.MaxValue) return 1f;
        if (best >= maxDistance) return 0f;

        // simple linear falloff, shaped by distanceFalloff
        float t = 1f - (best / Mathf.Max(0.0001f, maxDistance));
        return Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.01f, distanceFalloff));
    }
}