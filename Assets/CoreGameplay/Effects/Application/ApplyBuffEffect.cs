using Features.Effects.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using UnityEngine;


namespace Features.Effects.Application
{
    public sealed class ApplyBuffEffect : IEffect
    {
        private readonly BuffSO _buff;

        public ApplyBuffEffect(BuffSO buff)
        {
            _buff = buff;
        }

        public void Apply(EffectContext context)
        {
            if (_buff == null || context.Targets == null)
                return;

            foreach (var target in context.Targets)
            {
                if (target is not IBuffTarget buffTarget)
                    continue;

                buffTarget.BuffSystem.Add(
                    _buff,
                    source: context.Source,
                    lifetimeMode: BuffLifetimeMode.Duration
                );
            }
        }
    }

}
