using System;
using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;

/// <summary>
/// Здоровье игрока:
/// - базовые значения HP приходят из PlayerStats/статовой системы
/// - текущие HP/Shield хранятся здесь
/// - баффы на Max HP / Shield применяются здесь
/// - интегрируется с IHealthReceiver и IShieldReceiver
/// </summary>
public class PlayerHealth : MonoBehaviour, IHealthReceiver, IShieldReceiver
{
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action OnDeath;

    private PlayerStats stats;

    [Header("Base Shield")]
    [SerializeField] private float baseShield = 0f;

    public float CurrentHp { get; private set; }
    public float CurrentShield { get; private set; }

    private float bonusMaxHpAdd = 0f;
    private float bonusMaxHpMult = 1f;

    private float bonusShieldAdd = 0f;
    private float bonusShieldMult = 1f;

    public float MaxHp
    {
        get
        {
            float baseHp = stats != null
                ? stats.Stats.Health.MaxHp
                : 100f;

            return (baseHp + bonusMaxHpAdd) * bonusMaxHpMult;
        }
    }

    public float MaxShield =>
        (baseShield + bonusShieldAdd) * bonusShieldMult;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (!stats)
            Debug.LogWarning("[PlayerHealth] PlayerStats not found on the object – using fallback HP.", this);
    }

    private void Start()
    {
        CurrentHp = MaxHp;
        CurrentShield = MaxShield;

        NotifyAll();
    }

    public void SetMaxHp(float hp)
    {
        if (stats != null)
        {
            stats.Stats.Health.ApplyBase(hp);
        }

        CurrentHp = MaxHp;
        NotifyHp();
    }

    public void SetShield(float shield)
    {
        baseShield = shield;
        CurrentShield = MaxShield;
        NotifyShield();
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
        NotifyHp();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        if (CurrentShield > 0)
        {
            float absorbed = Mathf.Min(CurrentShield, damage);
            CurrentShield -= absorbed;
            damage -= absorbed;
            NotifyShield();
        }

        if (damage > 0)
        {
            CurrentHp -= damage;
            NotifyHp();
        }

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            OnDeath?.Invoke();
        }
    }

    public void ApplyMaxHpBuff(BuffSO cfg, bool apply)
    {
        float sign = apply ? 1f : -1f;

        switch (cfg.modType)
        {
            case BuffModType.Add:
                bonusMaxHpAdd += sign * cfg.value;
                break;

            case BuffModType.Mult:
                if (apply) bonusMaxHpMult *= cfg.value;
                else if (cfg.value != 0f) bonusMaxHpMult /= cfg.value;
                break;

            case BuffModType.Set:
                bonusMaxHpMult = apply ? cfg.value : 1f;
                break;
        }

        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        NotifyHp();
    }

    public void ApplyShieldBuff(BuffSO cfg, bool apply)
    {
        float sign = apply ? 1f : -1f;

        switch (cfg.modType)
        {
            case BuffModType.Add:
                bonusShieldAdd += sign * cfg.value;
                break;

            case BuffModType.Mult:
                if (apply) bonusShieldMult *= cfg.value;
                else if (cfg.value != 0f) bonusShieldMult /= cfg.value;
                break;

            case BuffModType.Set:
                bonusShieldMult = apply ? cfg.value : 1f;
                break;
        }

        CurrentShield = Mathf.Min(CurrentShield, MaxShield);
        NotifyShield();
    }

    private void NotifyHp() =>
        OnHealthChanged?.Invoke(CurrentHp, MaxHp);

    private void NotifyShield() =>
        OnShieldChanged?.Invoke(CurrentShield, MaxShield);

    private void NotifyAll()
    {
        NotifyHp();
        NotifyShield();
    }
}
