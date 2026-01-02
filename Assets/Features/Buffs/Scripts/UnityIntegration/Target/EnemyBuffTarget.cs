using UnityEngine;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;

public sealed class EnemyBuffTarget : NetworkBehaviour, IBuffTarget
{
    public BuffSystem BuffSystem { get; private set; }
    public IStatsFacade Stats { get; private set; }

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public bool IsReady => BuffSystem != null && Stats != null;

    public event System.Action OnReady;

    private bool _fired;

    private void Awake()
    {
        BuffSystem = GetComponent<BuffSystem>();
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

    public void SetStats(IStatsFacade stats)
    {
        Stats = stats;
        TryFireReady();
    }

    private void TryFireReady()
    {
        if (_fired || !IsReady)
            return;

        _fired = true;
        OnReady?.Invoke();
    }

    public IStatsFacade GetServerStats() => Stats;
}
