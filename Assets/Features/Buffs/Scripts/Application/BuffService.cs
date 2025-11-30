using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.Application
{
    public class BuffService
    {
        private readonly BuffExecutor _executor;
        private readonly List<BuffInstance> _active = new();

        public IReadOnlyList<BuffInstance> Active => _active;

        public event System.Action<BuffInstance> OnAdded;
        public event System.Action<BuffInstance> OnRemoved;

        public BuffService(BuffExecutor executor)
        {
            _executor = executor;
        }

        public BuffInstance AddBuff(BuffSO config, IBuffTarget target)
        {
            if (config == null || target == null || _executor == null)
                return null;

            // ищем существующий бафф
            var existing = _active.Find(x => x.Config == config && x.Target == target);

            if (existing != null)
            {
                if (config.isStackable)
                    existing.StackCount++;

                existing.Refresh();
                _executor.Apply(existing);
                OnAdded?.Invoke(existing);
                return existing;
            }

            // создаем новый
            var inst = new BuffInstance(config, target);
            _active.Add(inst);

            _executor.Apply(inst);
            OnAdded?.Invoke(inst);

            return inst;
        }

        public void RemoveBuff(BuffInstance inst)
        {
            if (inst == null || _executor == null)
                return;

            if (_active.Remove(inst))
            {
                _executor.Expire(inst);
                OnRemoved?.Invoke(inst);
            }
        }

        public void Tick(float dt)
        {
            if (_executor == null) return;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var inst = _active[i];

                // HealPerSecond
                _executor.Tick(inst, dt);

                if (inst.IsExpired)
                    RemoveBuff(inst);
            }
        }
    }
}
