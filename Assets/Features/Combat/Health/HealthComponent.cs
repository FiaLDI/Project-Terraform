using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHp = 100f;
    public float currentHp;

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDeath;

    private void Awake()
    {
        currentHp = maxHp;
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(float damageAmount, DamageType damageType)
    {
        if (damageAmount <= 0f) return;

        damageAmount = DamageSystem.ApplyDamageModifiers(
            this,
            damageAmount,
            damageType
        );

        currentHp -= damageAmount;
        OnHealthChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
            Die();
    }

    public void Heal(float healAmount)
    {
        if (healAmount <= 0f) return;

        currentHp = Mathf.Min(maxHp, currentHp + healAmount);
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
