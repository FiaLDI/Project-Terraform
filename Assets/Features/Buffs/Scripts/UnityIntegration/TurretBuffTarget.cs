using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;

public class TurretBuffTarget : MonoBehaviour, IBuffTarget
{
    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public BuffSystem BuffSystem { get; private set; }
    public IStatsFacade Stats { get; private set; }

    private TurretStats _stats;

    private void Awake()
    {
        // 1) TurretStats must exist
        _stats = GetComponent<TurretStats>();
        if (_stats == null)
        {
            Debug.LogError("[TurretBuffTarget] No TurretStats found!", this);
            return;
        }

        // 2) Ensure BuffSystem
        BuffSystem = GetComponent<BuffSystem>();
        if (BuffSystem == null)
            BuffSystem = gameObject.AddComponent<BuffSystem>();

        // 3) Связываем BuffSystem с таргетом
        BuffSystem.SetTarget(this);

        // 4) Пробрасываем фасад статов в IBuffTarget
        Stats = _stats.Facade;

        Debug.Log("[TurretBuffTarget] Bound successfully.", this);
    }
}
