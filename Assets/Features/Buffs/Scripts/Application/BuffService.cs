// Assets/Features/Buffs/Scripts/Application/BuffService.cs
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Бафф-сервис: отвечает за список активных баффов и их время жизни.
    /// </summary>
    public class BuffService
    {
        private readonly BuffExecutor executor;

        private readonly List<BuffInstance> active = new();
        public IReadOnlyList<BuffInstance> Active => active;

        public event Action<BuffInstance> OnAdded;
        public event Action<BuffInstance> OnRemoved;

        public BuffService(BuffExecutor executor)
        {
            this.executor = executor;
        }

        // =====================================================================
        // ADD
        // =====================================================================
        public BuffInstance AddBuff(BuffSO config, IBuffTarget target)
        {
            if (config == null || target == null)
                return null;

            var existing = FindExisting(config, target);

            // если уже есть бафф с таким stat+modType на этом таргете
            if (existing != null)
            {
                if (config.isStackable)
                {
                    existing.StackCount++;
                    existing.Refresh();
                    executor.Apply(existing);
                }
                else
                {
                    // нестакаемый — только продлеваем
                    existing.Refresh();
                }

                OnAdded?.Invoke(existing);
                return existing;
            }

            // создаём новый
            var inst = new BuffInstance(config, target);
            active.Add(inst);

            executor.Apply(inst);
            OnAdded?.Invoke(inst);

            return inst;
        }

        private BuffInstance FindExisting(BuffSO cfg, IBuffTarget target)
        {
            foreach (var inst in active)
            {
                if (inst.Target == target &&
                    inst.Config.stat == cfg.stat &&
                    inst.Config.modType == cfg.modType)
                {
                    return inst;
                }
            }

            return null;
        }

        // =====================================================================
        // REMOVE
        // =====================================================================
        public void RemoveBuff(BuffInstance inst)
        {
            if (inst == null)
                return;

            if (active.Remove(inst))
            {
                executor.Expire(inst);
                OnRemoved?.Invoke(inst);
            }
        }

        // =====================================================================
        // TICK
        // =====================================================================
        public void Tick(float dt)
        {
            if (active.Count == 0)
                return;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                var inst = active[i];

                // уменьшаем таймер
                inst.Tick(dt);

                // тики по времени (HealPerSecond и прочее)
                executor.Tick(inst, dt);

                if (inst.IsExpired)
                {
                    executor.Expire(inst);
                    OnRemoved?.Invoke(inst);
                    active.RemoveAt(i);
                }
            }
        }

        // =====================================================================
        // CLEAR
        // =====================================================================
        public void ClearAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                executor.Expire(active[i]);
                OnRemoved?.Invoke(active[i]);
            }

            active.Clear();
        }
    }
}
