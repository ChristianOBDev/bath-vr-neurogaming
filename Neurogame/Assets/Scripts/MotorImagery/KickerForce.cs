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

    [Header("Glow Build")]
    public AnimationCurve glowBuildCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Kick Swell")]
    public List<Transform> swellTargets = new List<Transform>();
    public float swellScale = 1.3f;
    public float swellDuration = 0.2f;
    public LeanTweenType swellEase = LeanTweenType.easeOutBack;

    [Header("Glow")]
    public bool glowEnabled = true;
    public Renderer glowRenderer;
    public Color glowColor = Color.cyan;
    public float maxGlowIntensity = 3f;
    public float glowLerpSpeed = 5f;

    [Header("Glow Phase 1 Pulse")]
    public float pulseSpeed = 1.5f;
    public float pulseMinIntensity = 0f;
    public float pulseMaxIntensity = 1f;

    private Material glowMaterial;
    private float currentGlowIntensity = 0f;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private Rigidbody currentBalloon;
    private BallController currentBallController;

    void Start()
    {
        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");
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

    void HandlePhaseChanged(GamePhase phase)
    {
        StopAllCoroutines();
        if (glowMaterial != null)
            glowMaterial.SetColor(EmissionColor, Color.black);
        currentGlowIntensity = 0f;
    }

    private Coroutine glowBuildCoroutine;

    public void BeginGlowBuild(float duration)
    {
        if (!glowEnabled || glowMaterial == null) return;
        if (glowBuildCoroutine != null) StopCoroutine(glowBuildCoroutine);
        glowBuildCoroutine = StartCoroutine(GlowBuildRoutine(duration));
    }

    public void TriggerKickSwell()
    {
        // Stop any ongoing glow build
        if (glowBuildCoroutine != null)
        {
            StopCoroutine(glowBuildCoroutine);
            glowBuildCoroutine = null;
        }

        // Flash glow to max then fade
        StartCoroutine(GlowFadeOut());

        // Swell each target mesh
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
    }

    IEnumerator GlowBuildRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = glowBuildCurve.Evaluate(elapsed / duration);
            float intensity = Mathf.Lerp(0f, maxGlowIntensity, t);
            Color finalColor = glowColor * Mathf.LinearToGammaSpace(intensity);
            glowMaterial.SetColor(EmissionColor, finalColor);
            yield return null;
        }

        // Ensure we reach full brightness
        glowMaterial.SetColor(EmissionColor, glowColor * Mathf.LinearToGammaSpace(maxGlowIntensity));
        glowBuildCoroutine = null;
    }

    IEnumerator GlowFadeOut()
    {
        float elapsed = 0f;
        float fadeDuration = 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / fadeDuration);
            float intensity = Mathf.Lerp(0f, maxGlowIntensity, t);
            Color finalColor = glowColor * Mathf.LinearToGammaSpace(intensity);
            glowMaterial.SetColor(EmissionColor, finalColor);
            yield return null;
        }

        glowMaterial.SetColor(EmissionColor, Color.black);
    }

    void OnCollisionEnter(Collision collision)
    {
        BallController ballController = collision.gameObject.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetState(BallState.Launching);
            currentBallController = ballController;
            TriggerKickSwell();
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