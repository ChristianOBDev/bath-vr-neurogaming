using System.Collections;
using UnityEngine;

public class BumperFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashDuration = 0.15f;
    public Color flashColor = Color.white;
    public float pulseScale = 1.4f;

    private Renderer rend;
    private Color originalEmission;
    private Vector3 originalScale;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;

        if (rend != null)
        {
            // Ensure emission is enabled on a material instance
            rend.material.EnableKeyword("_EMISSION");
            originalEmission = rend.material.GetColor(EmissionColor);
        }
    }

    public IEnumerator DoFlash()
    {
        float elapsed = 0f;
        Vector3 targetScale = originalScale * pulseScale;

        while (elapsed < flashDuration)
        {
            float t = elapsed / flashDuration;

            // Scale pulse: grow then shrink
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);

            // Emission flash: bright then fade
            if (rend != null)
                rend.material.SetColor(EmissionColor, Color.Lerp(flashColor, originalEmission, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        if (rend != null)
            rend.material.SetColor(EmissionColor, originalEmission);
    }
}