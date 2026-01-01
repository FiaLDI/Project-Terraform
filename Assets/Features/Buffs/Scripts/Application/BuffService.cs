// Assets/Features/Buffs/Scripts/Application/BuffService.cs
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    public sealed class BuffService
    {
        private readonly BuffExecutor executor;
        private readonly bool viewOnly;

        private readonly List<BuffInstance> active = new();
        public IReadOnlyList<BuffInstance> Active => active;

        public event Action<BuffInstance> OnAdded;
        public event Action<BuffInstance> OnRemoved;

        // ================= CONSTRUCTORS =================

        // 🔥 SERVER
        public BuffService(BuffExecutor executor)
        {
            this.executor = executor;
            this.viewOnly = false;
        }

        // 👁 CLIENT (view-only)
        private BuffService()
        {
            this.viewOnly = true;
        }

        public static BuffService CreateViewOnly()
        {
            return new BuffService();
        }

        // ================= ADD =================

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

            if (!viewOnly)
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

        // ================= REMOVE =================

        public void RemoveBuff(BuffInstance inst)
        {
            if (inst == null)
                return;

            if (active.Remove(inst))
            {
                if (!viewOnly)
                    executor.Expire(inst);

                OnRemoved?.Invoke(inst);
            }
        }

        // ================= TICK =================

        public void Tick(float dt)
        {
            if (viewOnly)
                return;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                var inst = active[i];

                inst.Tick(dt);
                executor.Tick(inst, dt);

                if (inst.IsExpired)
                    RemoveBuff(inst);
            }
        }

        // ================= CLEAR =================

        public void ClearAll()
        {
            if (!viewOnly)
            {
                for (int i = active.Count - 1; i >= 0; i--)
                {
                    executor.Expire(active[i]);
                    OnRemoved?.Invoke(active[i]);
                }
            }

            active.Clear();
        }
    }
}
