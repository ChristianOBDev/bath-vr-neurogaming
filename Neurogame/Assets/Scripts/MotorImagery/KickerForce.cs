using MotorImagery;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KickerForce : MonoBehaviour
{

    [Header("Debug")]
    public bool kickerDebugLogging = false;
    [Header("References")]
    public KickerInputRouter inputRouter;
    [Tooltip("True = Left (Blue), False = Right (Red)")]
    public bool isLeftKicker;

    [Header("Force Settings")]
    public float launchForce = 15f;
    [Range(0f, 1f)]
    public float lateralInfluence = 0.25f;
    [Range(45f, 90f)]
    public float minLaunchAngle = 70f;

    [Header("Velocity Control")]
    public float maxUpwardVelocity = 4.5f;
    public bool graduatedForce = true;

    [Header("Phase Settings")]
    [Range(0f, 1f)]
    public float minStrength = 0f;
    public bool popOnNoInput = false;

    [Header("Launch Direction")]
    [Tooltip("Launch direction in LOCAL space")]
    public Vector3 forwardDirection = Vector3.forward;

    [Header("Audio")]
    public AudioClip hitSound;

    [Header("Glow")]
    public bool glowEnabled = true;
    public Renderer glowRenderer;
    public Color glowColor = Color.cyan;
    public float maxGlowIntensity = 3f;
    public float glowLerpSpeed = 5f;

    [Header("Glow Phase 1")]
    public float glowBuildTimeOffset = 1f; // How many seconds before contact glow reaches 100%
    public AnimationCurve glowBuildCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float glowFadeOutDuration = 0.5f;

    [Header("Kick Swell")]
    public List<Transform> swellTargets = new List<Transform>();
    public float swellScale = 1.3f;
    public float swellDuration = 0.2f;
    public LeanTweenType swellEase = LeanTweenType.easeOutBack;

    private Material glowMaterial;
    //private float currentGlowIntensity = 0f;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private Rigidbody currentBalloon;
    private BallController currentBallController;

    void Start()
    {
        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");
            SetGlowIntensity(0f); // must be first
        }

        // Log current phase for debugging
        if (PhaseManager.Instance == null)
            Debug.LogWarning("PhaseManager is null at KickerForce Start!");
        else
            Debug.Log($"KickerForce Start - Phase: {PhaseManager.Instance.CurrentPhase}");

        // Subscribe to phase changes
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;

        // Apply initial phase state
        if (PhaseManager.Instance != null)
            HandlePhaseChanged(PhaseManager.Instance.CurrentPhase);
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    private Coroutine glowCoroutine;

    public void BeginGlowBuild(float returnDuration)
    {
        if (!glowEnabled || glowMaterial == null) return;
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);

        // Build duration is returnDuration minus the offset
        // so glow reaches 100% 'glowBuildTimeOffset' seconds before contact
        float buildDuration = Mathf.Max(0.1f, returnDuration - glowBuildTimeOffset);
        Debug.Log($"BeginGlowBuild called. returnDuration: {returnDuration}, buildDuration: {buildDuration}");

        glowCoroutine = StartCoroutine(DelayedGlowBuild(buildDuration));
    }

    IEnumerator DelayedGlowBuild(float buildDuration)
    {
        // Wait for all Start() methods to complete before beginning
        yield return null;
        yield return null;
        glowCoroutine = StartCoroutine(GlowBuildRoutine(buildDuration));
    }

    public void TriggerKickEffect()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        // Swell meshes
        foreach (Transform target in swellTargets)
        {
            if (target == null) continue;
            Vector3 originalScale = target.localScale;
            LeanTween.cancel(target.gameObject);
            LeanTween.scale(target.gameObject, originalScale * swellScale, swellDuration * 0.3f)
                .setEase(swellEase)
                .setOnComplete(() =>
                {
                    LeanTween.scale(target.gameObject, originalScale, swellDuration * 0.7f)
                        .setEase(LeanTweenType.easeOutQuad);
                });
        }

        // Fade glow out after contact
        glowCoroutine = StartCoroutine(GlowFadeRoutine(maxGlowIntensity, 0f, glowFadeOutDuration));
    }

    IEnumerator GlowBuildRoutine(float duration)
    {
        Debug.Log($"GlowBuildRoutine started with duration: {duration}");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = glowBuildCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            SetGlowIntensity(Mathf.Lerp(0f, maxGlowIntensity, t));
            yield return null;
        }

        Debug.Log($"GlowBuildRoutine completed. Elapsed: {elapsed}");
        SetGlowIntensity(maxGlowIntensity);
        glowCoroutine = null;
    }

    IEnumerator GlowFadeRoutine(float fromIntensity, float toIntensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetGlowIntensity(Mathf.Lerp(fromIntensity, toIntensity, t));
            yield return null;
        }

        SetGlowIntensity(toIntensity);
        glowCoroutine = null;
    }

    void SetGlowIntensity(float intensity)
    {
        if (glowMaterial == null) return;
        if (kickerDebugLogging)
            Debug.Log($"SetGlowIntensity: {intensity}");
        Color finalColor = glowColor * Mathf.LinearToGammaSpace(intensity);
        glowMaterial.SetColor(EmissionColor, finalColor);
    }

    void UpdateGlowFromInput()
    {
        if (!glowEnabled || glowMaterial == null) return;
        if (inputRouter == null) return;

        float strength = inputRouter.GetStrength(isLeftKicker, graduatedForce);
        float targetIntensity = strength * maxGlowIntensity;
        float currentIntensity = Mathf.Lerp(
            GetCurrentGlowIntensity(),
            targetIntensity,
            Time.deltaTime * glowLerpSpeed
        );
        SetGlowIntensity(currentIntensity);
    }

    float GetCurrentGlowIntensity()
    {
        if (glowMaterial == null) return 0f;
        Color current = glowMaterial.GetColor(EmissionColor);
        return current.maxColorComponent;
    }

    void HandlePhaseChanged(GamePhase phase)
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }
        SetGlowIntensity(0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        BallController ballController = collision.gameObject.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetState(BallState.Launching);
            currentBallController = ballController;
            TriggerKickEffect();
        }
        if (collision.rigidbody != null)
            currentBalloon = collision.rigidbody;

        if (hitSound != null)
            GetComponent<AudioSource>().PlayOneShot(hitSound);

        if (kickerDebugLogging)
            Debug.Log($"Kicker collision entered. Balloon: {currentBalloon?.name}, Strength: {inputRouter?.GetStrength(isLeftKicker, graduatedForce)}");
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == currentBalloon)
        {
            currentBalloon = null;
            currentBallController = null;
        }
        if (kickerDebugLogging)
            Debug.Log($"Kicker collision exited. Balloon was: {collision.rigidbody?.name}");
    }

    void FixedUpdate()
    {
        if (currentBalloon == null || inputRouter == null) return;

        if (kickerDebugLogging)
            Debug.Log($"Kicker FixedUpdate - strength: {Mathf.Max(inputRouter.GetStrength(isLeftKicker, graduatedForce), minStrength)}, state: {currentBallController?.CurrentState}");
            Debug.Log($"PhaseManager null: {PhaseManager.Instance == null}, Phase: {PhaseManager.Instance?.CurrentPhase}");

        float playerStrength = inputRouter.GetStrength(isLeftKicker, graduatedForce);

        if (popOnNoInput && playerStrength <= 0f)
        {
            if (currentBallController != null && GameManager.Instance != null)
            {
                GameManager.Instance.HandleBallDeath(currentBallController);
                currentBalloon = null;
                currentBallController = null;
            }
            return;
        }

        // Input driven glow for Phase 2 and 3
        if (PhaseManager.Instance != null &&
            PhaseManager.Instance.CurrentPhase != GamePhase.PhaseOne)
        {
            UpdateGlowFromInput();
        }

        if (currentBalloon == null || inputRouter == null) return;

        //Float strength with minimum threshold
        float strength = Mathf.Max(playerStrength, minStrength);
        if (strength <= 0f) return;

        // Forward direction is defined
        Vector3 worldForward = transform.TransformDirection(forwardDirection.normalized);

        Vector3 upAxis = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.Up
            : Vector3.up;

        // Build launch direction
        Vector3 direction = worldForward;
        float upY = Mathf.Tan(minLaunchAngle * Mathf.Deg2Rad);
        direction += upAxis * upY;

        // Add lateral influence
        Vector3 contactOffset = currentBalloon.position - transform.position;
        Vector3 lateral = Vector3.ProjectOnPlane(contactOffset, upAxis).normalized;
        direction += lateral * lateralInfluence;
        direction.Normalize();

        currentBalloon.AddForce(direction * launchForce * strength, ForceMode.Force);

        // Clamp upward velocity
        Vector3 v = currentBalloon.linearVelocity;
        float upSpeed = Vector3.Dot(v, upAxis);
        if (upSpeed > maxUpwardVelocity)
        {
            v -= upAxis * (upSpeed - maxUpwardVelocity);
            currentBalloon.linearVelocity = v;
        }
    }
}