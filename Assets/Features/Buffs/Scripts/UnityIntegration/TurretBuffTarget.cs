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
    public IStatsFacade Stats { get; private set; }

    private TurretStats _stats;

    private void Awake()
    {
        _stats = GetComponent<TurretStats>();
        if (_stats == null)
        {
            Debug.LogError("[TurretBuffTarget] No TurretStats found!", this);
            return;
        }

        BuffSystem = GetComponent<BuffSystem>();
        if (BuffSystem == null)
        {
            Debug.LogError("[TurretBuffTarget] BuffSystem missing!", this);
            return;
        }

        Stats = _stats.Facade;
    }

    // 🔥 КЛЮЧЕВОЕ МЕСТО
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
}
