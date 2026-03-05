using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class KickerForce : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip hitSound;

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
    public Vector3 forwardDirection = Vector3.forward;

    [Header("Glow")]
    public bool glowEnabled = true;
    public Renderer glowRenderer;
    public Color glowColor = Color.cyan;
    public float maxGlowIntensity = 3f;
    public float glowLerpSpeed = 5f;

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
    }

    void OnCollisionEnter(Collision collision)
    {
        BallController ballController = collision.gameObject.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetState(BallState.Launching);
            currentBallController = ballController;
        }
        if (collision.rigidbody != null)
            currentBalloon = collision.rigidbody;

        if (hitSound != null)
            GetComponent<AudioSource>().PlayOneShot(hitSound);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody == currentBalloon)
        {
            currentBalloon = null;
            currentBallController = null;
        }
    }

    void FixedUpdate()
    {
        if (currentBalloon == null || inputRouter == null) return;

        float playerStrength = inputRouter.GetStrength(isLeftKicker, graduatedForce);

        // Phase 3 ball pop — if no input and popOnNoInput is set
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

        // Apply minimum strength floor
        float strength = Mathf.Max(playerStrength, minStrength);

        if (strength <= 0f) return;

        Vector3 direction = forwardDirection.normalized;
        float upY = Mathf.Tan(minLaunchAngle * Mathf.Deg2Rad);
        direction.y += upY;

        Vector3 contactOffset = currentBalloon.position - transform.position;
        Vector3 lateral = Vector3.ProjectOnPlane(contactOffset, Vector3.up).normalized;
        direction += lateral * lateralInfluence;
        direction.Normalize();

        currentBalloon.AddForce(direction * launchForce * strength, ForceMode.Force);

        Vector3 v = currentBalloon.linearVelocity;
        if (v.y > maxUpwardVelocity)
        {
            v.y = maxUpwardVelocity;
            currentBalloon.linearVelocity = v;
        }

        UpdateGlow(strength);
    }

    void UpdateGlow(float strength)
    {
        if (!glowEnabled || glowMaterial == null) return;

        float targetIntensity = strength * maxGlowIntensity;
        currentGlowIntensity = Mathf.Lerp(currentGlowIntensity, targetIntensity, Time.deltaTime * glowLerpSpeed);

        Color finalColor = glowColor * Mathf.LinearToGammaSpace(currentGlowIntensity);
        glowMaterial.SetColor(EmissionColor, finalColor);
        glowRenderer.enabled = glowEnabled;
    }
}