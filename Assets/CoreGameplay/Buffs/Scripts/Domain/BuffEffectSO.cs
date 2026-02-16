using Features.Buffs.Application;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Buffs.Domain
{
    public abstract class BuffEffectSO : ScriptableObject, IBuffEffect
    {
        public abstract void Apply(IStatsFacade stats);
        public virtual void Tick(IStatsFacade stats, float dt) { }
        public abstract void Expire(IStatsFacade stats);

        public virtual void ApplyWithContext(BuffInstance inst, IStatsFacade stats)
        {
            Apply(stats);
        }

    }
}
