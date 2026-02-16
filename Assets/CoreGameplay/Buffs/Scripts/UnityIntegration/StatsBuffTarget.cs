using System;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(StatsOwnerBase))]
[RequireComponent(typeof(BuffSystem))]
public sealed class StatsBuffTarget : NetworkBehaviour, IBuffTarget, IBuffSource
{
    private StatsOwnerBase statsOwner;
    private bool fired;

    // ================= IBuffTarget =================

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public BuffSystem BuffSystem { get; private set; }

    public bool IsReady => statsOwner != null && statsOwner.IsReady && BuffSystem != null;
    public event Action OnReady;

    public IBuffSource OwnerSource
    {
        get
        {
            // Если это турель — берём владельца из TurretStats
            var turret = GetComponent<TurretStats>();
            if (turret != null)
                return turret.GetOwnerSource();

            // Если игрок — сам себе владелец
            return this;
        }
    }


    // ================= LIFECYCLE =================

    private void Awake()
    {
        BuffSystem = GetComponent<BuffSystem>();
        statsOwner = GetComponent<StatsOwnerBase>();

        if (statsOwner == null)
            Debug.LogError("[StatsBuffTarget] StatsOwnerBase missing", this);
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

    // ================= READY =================

    private void TryFireReady()
    {
        if (fired || !IsReady)
            return;

        fired = true;
        OnReady?.Invoke();
    }

    // ================= STATS =================

    public IStatsFacade GetServerStats()
    {
        return IsServerStarted ? statsOwner.Facade : null;
    }
}
