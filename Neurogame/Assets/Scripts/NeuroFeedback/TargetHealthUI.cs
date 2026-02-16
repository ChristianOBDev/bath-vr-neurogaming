using UnityEngine;
using UnityEngine.UI;

public class TargetHealthBarUI : MonoBehaviour
{
    [SerializeField] private TargetHealth targetHealth;
    [SerializeField] private Image fillImage; // Image Type = Filled, Horizontal

    private void Awake()
    {
        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();
    }

    private void OnEnable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged += HandleHealthChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(TargetHealth th) => Refresh();

    private void Refresh()
    {
        if (targetHealth == null || fillImage == null) return;

        float pct = (targetHealth.MaxHealth <= 0f) ? 0f : targetHealth.Health / targetHealth.MaxHealth;
        fillImage.fillAmount = Mathf.Clamp01(pct);
    }
}
