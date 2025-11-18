using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHp = 120f;
    public float CurrentHp { get; private set; }

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDeath;

    private void Awake()
    {
        CurrentHp = maxHp;
        OnHealthChanged?.Invoke(CurrentHp, maxHp);
    }

    public void TakeDamage(float amount, DamageType type)
    {
        CurrentHp -= amount;
        OnHealthChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp <= 0)
            Die();
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
}
