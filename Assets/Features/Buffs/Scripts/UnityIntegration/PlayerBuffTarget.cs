using UnityEngine;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;

public class PlayerBuffTarget : MonoBehaviour, IBuffTarget
{
    public BuffSystem BuffSystem { get; private set; }
    public IStatsFacade Stats { get; private set; }

    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        BuffSystem = GetComponent<BuffSystem>();
        if (BuffSystem == null)
            Debug.LogError("[PlayerBuffTarget] Missing BuffSystem");
    }

    // вызывается PlayerClassController
    public void SetStats(IStatsFacade stats)
    {
        Stats = stats;
    }
}
