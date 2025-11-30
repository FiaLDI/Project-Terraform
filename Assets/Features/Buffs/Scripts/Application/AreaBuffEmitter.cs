using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    public class AreaBuffEmitter : MonoBehaviour
    {
        public AreaBuffSO area;
        private readonly Dictionary<IBuffTarget, BuffInstance> active = new();

        private void Update()
        {
            if (area == null || area.buff == null)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                area.radius,
                area.targetMask
            );

            HashSet<IBuffTarget> inside = new();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent<IBuffTarget>(out var target))
                    continue;

                inside.Add(target);

                if (!active.ContainsKey(target))
                {
                    BuffInstance inst = target.BuffSystem.Add(area.buff);
                    active[target] = inst;
                }
            }

            List<IBuffTarget> toRemove = new();

            foreach (var kv in active)
            {
                if (!inside.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var t in toRemove)
            {
                t.BuffSystem.Remove(active[t]);
                active.Remove(t);
            }
        }
    }
}
