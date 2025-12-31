using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.UnityIntegration;
using Features.Stats.Domain;

[RequireComponent(typeof(BuffSystem))]
public class TurretBuffTarget : MonoBehaviour, IBuffTarget
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

        Debug.Log("[TurretBuffTarget] Bound successfully.", this);
    }
}
