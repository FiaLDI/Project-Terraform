using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Combat.Domain;

public sealed class HealthNetwork : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float startMaxHp = 100f;

    [Header("Resistances (optional)")]
    [SerializeField] private ResistProfile resistances;

    private readonly SyncVar<float> _maxHp = new();
    private readonly SyncVar<float> _currentHp = new();

    public float MaxHp => _maxHp.Value;
    public float CurrentHp => _currentHp.Value;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _maxHp.OnChange += (_, __, ___) => RaiseChanged();
        _currentHp.OnChange += (_, __, ___) => RaiseChanged();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _maxHp.Value = startMaxHp;
        _currentHp.Value = startMaxHp;
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        OnHealthChanged?.Invoke(_currentHp.Value, _maxHp.Value);
    }

    public ResistProfile GetResistProfile() => resistances;

    public void ApplyDamage(float amount, DamageType type, HitInfo info) => TakeDamage(amount, type);

    public void TakeDamage(float damageAmount, DamageType damageType)
    {
        if (!IsServerInitialized || damageAmount <= 0f)
            return;

        _currentHp.Value = Mathf.Max(0f, _currentHp.Value - damageAmount);

        if (_currentHp.Value <= 0f)
            DieServer();
    }

    public void Heal(float healAmount)
    {
        if (!IsServerInitialized || healAmount <= 0f)
            return;

        _currentHp.Value = Mathf.Min(_maxHp.Value, _currentHp.Value + healAmount);
    }

    private void DieServer()
    {
        OnDeath?.Invoke();
        Despawn();
    }
}
