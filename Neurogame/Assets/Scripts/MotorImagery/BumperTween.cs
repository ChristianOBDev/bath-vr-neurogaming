using UnityEngine;

public class BumperTween : MonoBehaviour
{
    [Header("Tween Settings")]
    public float duration = 0.5f;
    public LeanTweenType easeType = LeanTweenType.easeOutBack;

    private Vector3 targetScale;
    private bool hasInitialized = false;

    void Awake()
    {
        // Store target scale once during initial prefab setup
        if (!hasInitialized)
        {
            targetScale = transform.localScale;
            hasInitialized = true;
        }
    }

    public void PlaySpawnTween()
    {
        // Cancel any existing tween on this gameobject
        LeanTween.cancel(gameObject);

        // Ensure we have a valid target scale (in case bumper was corrupted)
        if (targetScale == Vector3.zero)
            targetScale = Vector3.one;

        // Set to zero and tween to target
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, targetScale, duration).setEase(easeType);
    }

    public void ResetTweenState()
    {
        // Force cancel any active tweens
        LeanTween.cancel(gameObject);

        // Restore to target scale immediately
        if (targetScale != Vector3.zero)
            transform.localScale = targetScale;
        else
            transform.localScale = Vector3.one;
    }
}