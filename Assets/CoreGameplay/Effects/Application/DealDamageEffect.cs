using Features.Combat.Domain;
using Features.Effects.Domain;
using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class DealDamageEffect : IEffect
    {
        private readonly float _damage;

        public DealDamageEffect(float damage)
        {
            _damage = damage;
        }

        public void Apply(EffectContext context)
        {
            foreach (var target in context.Targets)
            {
                if (target is IDamageable dmg)
                    dmg.TakeDamage(_damage, DamageType.Generic);
            }
        }
    }
}
