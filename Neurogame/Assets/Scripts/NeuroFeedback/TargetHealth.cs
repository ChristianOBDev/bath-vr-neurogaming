using System;
using UnityEngine;

public class TargetHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float respawnDelay = 3f;

    public float MaxHealth => maxHealth;
    public float Health { get; private set; }
    public float RespawnDelay => respawnDelay;

    // Alive based on health, not active state (we keep root active)
    public bool IsAlive => Health > 0f;

    public event Action<TargetHealth> OnHealthChanged;
    public event Action<TargetHealth> OnKilled;

    private void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        Health = maxHealth;
        OnHealthChanged?.Invoke(this);
    }

    public void ApplyDamage(float dmg)
    {
        if (!IsAlive) return;

        Health = Mathf.Max(0f, Health - dmg);
        OnHealthChanged?.Invoke(this);

        if (Health <= 0f)
            OnKilled?.Invoke(this);
    }
}
