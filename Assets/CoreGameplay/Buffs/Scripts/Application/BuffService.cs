// Assets/Features/Buffs/Scripts/Application/BuffService.cs
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Хранилище и таймер баффов.
    /// ❗ НЕ применяет эффекты и НЕ знает про Executor.
    /// </summary>
    public sealed class BuffService
    {
        private readonly List<BuffInstance> active = new();
        public IReadOnlyList<BuffInstance> Active => active;

        public event Action<BuffInstance> OnAdded;
        public event Action<BuffInstance> OnRemoved;

        // =====================================================
        // ADD
        // =====================================================

        public BuffInstance AddBuff(
            BuffSO cfg,
            IBuffTarget target,
            IBuffSource source,
            BuffLifetimeMode lifetimeMode)
        {
            if (cfg == null || target == null)
                return null;

            var inst = new BuffInstance(cfg, target, source, lifetimeMode);
            active.Add(inst);

            OnAdded?.Invoke(inst);
            return inst;
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public void RemoveBuff(BuffInstance inst)
        {
            if (inst == null)
                return;

            if (active.Remove(inst))
                OnRemoved?.Invoke(inst);
        }

        public void RemoveBySource(IBuffSource source)
        {
            for (int i = active.Count - 1; i >= 0; i--)
                if (active[i].Source == source)
                    RemoveBuff(active[i]);
        }

        // =====================================================
        // TICK
        // =====================================================

        public void Tick(float dt)
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var inst = active[i];
                inst.Tick(dt);

                if (inst.IsExpired)
                    RemoveBuff(inst);
            }
        }

        // =====================================================
        // CLEAR
        // =====================================================

        public void ClearAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
                OnRemoved?.Invoke(active[i]);

            active.Clear();
        }
    }
}
