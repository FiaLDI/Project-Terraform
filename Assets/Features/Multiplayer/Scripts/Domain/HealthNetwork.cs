using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Stats.Domain;

public sealed class HealthNetwork : NetworkBehaviour
{
    private readonly SyncVar<float> _maxHp = new();
    private readonly SyncVar<float> _currentHp = new();

    private IStatsOwner statsOwner;
    private IHealthStats health;

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

        statsOwner = GetComponent<IStatsOwner>();
        if (statsOwner == null || !statsOwner.IsReady)
        {
            Debug.LogError("[HealthNetwork] StatsOwner not ready", this);
            return;
        }

        health = statsOwner.Facade.Health;

        if (health == null)
        {
            Debug.LogError("[HealthNetwork] HealthStats missing", this);
            return;
        }

        // initial sync
        _maxHp.Value = health.MaxHp;
        _currentHp.Value = health.CurrentHp;

        health.OnHealthChanged += HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (!IsServerInitialized)
            return;

        _maxHp.Value = max;
        _currentHp.Value = current;

        if (current <= 0f)
            DieServer();
    }

    private void RaiseChanged()
    {
        OnHealthChanged?.Invoke(_currentHp.Value, _maxHp.Value);
    }

    private void DieServer()
    {
        OnDeath?.Invoke();
        Despawn();
    }
}
