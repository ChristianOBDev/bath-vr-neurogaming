using System;
using UnityEngine;
using UnityEngine.UI;

public class NeuroTargetHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI (optional)")]
    public Image healthFill; // horizontal filled image

    public bool IsAlive => currentHealth > 0f;

    public event Action<NeuroTargetHealth> OnKilled;

    void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void ApplyDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            OnKilled?.Invoke(this);
        }
    }

    private void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = maxHealth <= 0 ? 0 : currentHealth / maxHealth;
    }
}