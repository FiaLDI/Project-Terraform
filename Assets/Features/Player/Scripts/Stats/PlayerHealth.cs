using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHp = 120f;
    public float CurrentHp { get; private set; }

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDeath;

    private float shield = 0f;

    private void Awake()
    {
        CurrentHp = maxHp;
        OnHealthChanged?.Invoke(CurrentHp, maxHp);
    }

    // =============================
    // DAMAGE LOGIC WITH SHIELD
    // =============================
    public void TakeDamage(float amount, DamageType type)
    {
        // 1 — щит поглощает
        if (shield > 0f)
        {
            float absorbed = Mathf.Min(shield, amount);
            shield -= absorbed;
            amount -= absorbed;
        }

        // 2 — остаток уходит в здоровье
        if (amount > 0f)
        {
            CurrentHp -= amount;
            OnHealthChanged?.Invoke(CurrentHp, maxHp);

            if (CurrentHp <= 0f)
                Die();
        }
    }

    public void Heal(float amount)
    {
        CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
        OnHealthChanged?.Invoke(CurrentHp, maxHp);
    }

    private void Die()
    {
        Debug.Log("Player died");
        OnDeath?.Invoke();
    }

    // =============================
    // SHIELD API
    // =============================
    public void AddShield(float amount)
    {
        shield += amount;
    }

    public void RemoveShield(float amount)
    {
        shield -= amount;
        if (shield < 0f) shield = 0f;
    }

    public float GetShield()
    {
        return shield;
    }
}
