using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Buffs.UnityIntegration
{
    public class AreaBuffEmitter : MonoBehaviour
    {
        public AreaBuffSO area;

        private readonly Dictionary<IBuffTarget, BuffInstance> _active = new();

        private void Update()
        {
            if (area == null || area.buff == null)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                area.radius,
                area.targetMask
            );

            var inside = new HashSet<IBuffTarget>();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent<IBuffTarget>(out var target))
                    continue;

                inside.Add(target);

                if (!_active.ContainsKey(target))
                {
                    var inst = target.BuffSystem.Add(area.buff);
                    if (inst != null)
                        _active[target] = inst;
                }
                else
                {
                    // Для НЕ-стакающихся баффов просто обновляем таймер
                    if (!area.buff.isStackable)
                        _active[target].Refresh();
                }
            }

            // выходим из радиуса
            var toRemove = new List<IBuffTarget>();

            foreach (var kv in _active)
            {
                if (!inside.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var t in toRemove)
            {
                if (t.BuffSystem != null)
                    t.BuffSystem.Remove(_active[t]);

                _active.Remove(t);
            }
        }
    }
}
