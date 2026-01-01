using UnityEngine;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;

[RequireComponent(typeof(BuffSystem))]
public sealed class TurretBuffTarget : NetworkBehaviour, IBuffTarget
{
    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public BuffSystem BuffSystem { get; private set; }

    private TurretStats _turretStats;

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
    }

    public override void OnStopServer()
    {
        ServerBuffTargetRegistry.Unregister(this);
        base.OnStopServer();
    }

    // =====================================================
    // IBuffTarget
    // =====================================================

    /// <summary>
    /// ⚠️ КРИТИЧЕСКИ ВАЖНО
    /// Возвращает ТОЛЬКО серверные статы турели
    /// </summary>
    public IStatsFacade GetServerStats()
    {
        if (!IsServerStarted)
            return null;

        return _turretStats?.Facade;
    }
}
