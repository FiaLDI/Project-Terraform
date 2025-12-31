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
        public BuffInstance AddBuff(
            BuffSO cfg,
            IBuffTarget target,
            IBuffSource source,
            BuffLifetimeMode lifetimeMode)
        {
            var inst = new BuffInstance(cfg, target, source, lifetimeMode);
            active.Add(inst);

            executor.Apply(inst);
            OnAdded?.Invoke(inst);

            return inst;
        }

        public void RemoveBySource(IBuffSource source)
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i].Source == source)
                    RemoveBuff(active[i]);
            }
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
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var inst = active[i];

                // ⏱ уменьшаем время
                inst.Tick(dt);

                // 🔥 тиковые эффекты (HoT / DoT)
                executor.Tick(inst, dt);

                // ❌ истёк по времени
                if (inst.IsExpired)
                {
                    RemoveBuff(inst);
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
