using System.Collections.Generic;
using UnityEngine;

public class BuffSystem : MonoBehaviour
{
    private readonly List<BuffInstance> active = new();
    public IReadOnlyList<BuffInstance> Active => active;
    public event System.Action<BuffInstance> OnBuffAdded;
    public event System.Action<BuffInstance> OnBuffRemoved;

    private IBuffTarget target;

    private void Awake()
    {
        target = GetComponent<IBuffTarget>() ??
                 GetComponentInChildren<IBuffTarget>() ??
                 GetComponentInParent<IBuffTarget>();

        if (target == null)
            Debug.LogError($"BuffSystem ERROR: нет IBuffTarget на объекте '{name}'");
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var inst = active[i];

            inst.Config.OnTick(inst, dt);

            if (inst.IsExpired)
                RemoveBuff(inst);
        }
    }

    public BuffInstance AddBuff(BuffSO config)
    {
        if (config == null) return null;
        if (target == null) return null;

        var existing = active.Find(x => x.Config == config);

        if (existing != null)
        {
            if (config.isStackable)
                existing.StackCount++;

            existing.Refresh(config.duration);

            OnBuffAdded?.Invoke(existing);

            return existing;
        }

        var inst = new BuffInstance(config, target);
        active.Add(inst);

        config.OnApply(inst);

        OnBuffAdded?.Invoke(inst);

        return inst;
    }


    public void RemoveBuff(BuffInstance inst)
    {
        if (active.Remove(inst))
        {
            inst.Config.OnExpire(inst);

            OnBuffRemoved?.Invoke(inst);
        }
    }

}