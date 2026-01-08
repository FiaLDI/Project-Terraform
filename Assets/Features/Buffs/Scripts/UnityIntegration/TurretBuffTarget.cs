using UnityEngine;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using System;

[RequireComponent(typeof(BuffSystem))]
[RequireComponent(typeof(TurretStats))]
public sealed class TurretBuffTarget : NetworkBehaviour, IBuffTarget
{
    // =====================================================
    // IBuffTarget PROPS
    // =====================================================

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public BuffSystem BuffSystem { get; private set; }

    public bool IsReady => BuffSystem != null && _turretStats != null;

    public event Action OnReady;

    // =====================================================
    // INTERNAL
    // =====================================================

    private TurretStats _turretStats;
    private bool _fired;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Awake()
    {
        BuffSystem = GetComponent<BuffSystem>();
        if (BuffSystem == null)
            Debug.LogError("[TurretBuffTarget] BuffSystem missing!", this);

        _turretStats = GetComponent<TurretStats>();
        if (_turretStats == null)
            Debug.LogError("[TurretBuffTarget] TurretStats missing!", this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerBuffTargetRegistry.Register(this);
        TryFireReady();
    }

    public override void OnStopServer()
    {
        ServerBuffTargetRegistry.Unregister(this);
        base.OnStopServer();
    }

    // =====================================================
    // READY LOGIC
    // =====================================================

    private void TryFireReady()
    {
        if (_fired || !IsReady)
            return;

        _fired = true;
        OnReady?.Invoke();
    }

    // =====================================================
    // IBuffTarget
    // =====================================================

    /// <summary>
    /// ⚠️ СЕРВЕР-ONLY
    /// </summary>
    public IStatsFacade GetServerStats()
    {
        if (!IsServerStarted)
            return null;

        return _turretStats?.Facade;
    }
}
