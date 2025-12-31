using UnityEngine;
using FishNet.Object;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;

public sealed class PlayerBuffTarget : NetworkBehaviour, IBuffTarget
{
    public BuffSystem BuffSystem { get; private set; }
    public IStatsFacade Stats { get; private set; }

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        BuffSystem = GetComponent<BuffSystem>();
        if (BuffSystem == null)
            Debug.LogError("[PlayerBuffTarget] Missing BuffSystem", this);
    }

    // ❗ ТОЛЬКО СЕРВЕР
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

    // вызывается PlayerClassController
    public void SetStats(IStatsFacade stats)
    {
        Stats = stats;
    }
}
