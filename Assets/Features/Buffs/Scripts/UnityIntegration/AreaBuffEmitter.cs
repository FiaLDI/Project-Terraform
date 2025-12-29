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
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    // НОВОЕ: Проверяем есть ли уже этот бафф
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    var existingBuff = FindExistingBuff(target);
                    
                    if (existingBuff != null)
                    {
                        // Бафф уже есть (например, от пассивов) - просто отслеживаем его
                        _active[target] = existingBuff;
                        if (!area.buff.isStackable)
                            existingBuff.Refresh();
                    }
                    else
                    {
                        // Баффа нет - добавляем новый
                        var inst = target.BuffSystem.Add(area.buff);
                        if (inst != null)
                            _active[target] = inst;
                    }
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

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // НОВОЕ: Когда AreaBuffEmitter удаляется, удаляем все его баффы
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private void OnDestroy()
        {
            // Удаляем все баффы которые мы добавили
            var toRemove = new List<IBuffTarget>(_active.Keys);
            
            foreach (var target in toRemove)
            {
                if (target?.BuffSystem != null && _active.TryGetValue(target, out var buff))
                {
                    target.BuffSystem.Remove(buff);
                }
                _active.Remove(target);
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // Проверяет есть ли уже этот бафф в BuffSystem
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private BuffInstance FindExistingBuff(IBuffTarget target)
        {
            if (target?.BuffSystem?.Active == null)
                return null;

            var buffId = area.buff.buffId;

            foreach (var buff in target.BuffSystem.Active)
            {
                if (buff?.Config?.buffId == buffId)
                    return buff;
            }

            return null;
        }
    }
}