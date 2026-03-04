using UnityEngine;

public class BumperTween : MonoBehaviour
{
    [Header("Tween Settings")]
    public float duration = 0.5f;
    public LeanTweenType easeType = LeanTweenType.easeOutBack;

    void Start()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, targetScale, duration).setEase(easeType);
    }
}