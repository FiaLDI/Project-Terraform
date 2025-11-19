using UnityEngine;
using System.Collections.Generic;

public class AreaBuffEmitter : MonoBehaviour
{
    public AreaBuffSO area;
    private readonly Dictionary<IBuffTarget, BuffInstance> active = new();

    private void Update()
    {
        if (area == null || area.buff == null) return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            area.radius,
            area.targetMask
        );

        HashSet<IBuffTarget> inside = new();

        foreach (var h in hits)
        {
            if (h.TryGetComponent<IBuffTarget>(out var target))
            {
                inside.Add(target);

                if (!active.ContainsKey(target))
                {
                    var inst = target.BuffSystem.AddBuff(area.buff);
                    active[target] = inst;
                }
            }
        }

        // remove those who left
        List<IBuffTarget> toRemove = new();
        foreach (var kv in active)
        {
            if (!inside.Contains(kv.Key))
                toRemove.Add(kv.Key);
        }

        foreach (var t in toRemove)
        {
            t.BuffSystem.RemoveBuff(active[t]);
            active.Remove(t);
        }
    }
}
