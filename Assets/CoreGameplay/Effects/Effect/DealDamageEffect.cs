using Features.Effects.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class DealDamageEffect : IEffect
    {
        private readonly float _value;
        private readonly DamageType _type;

        public DealDamageEffect(float value, DamageType type)
        {
            _value = value;
            _type = type;
        }

        public void Apply(EffectContext context)
        {
            
            if (context.Targets == null)
                return;

            foreach (var t in context.Targets)
            {
                if (t?.BuffSystem == null || !t.IsReady)
                    continue;

                var statsOwner = t.BuffSystem.GetComponentInParent<IStatsOwner>();

                if (statsOwner == null || !statsOwner.IsReady)
                    continue;

                var stats = statsOwner.Facade;
                stats?.Health?.Damage(_value);

                var enemy = t.BuffSystem.GetComponentInParent<EnemyStats>();

                if (enemy != null && context.Source != null)
                {
                    enemy.RegisterAttacker(context.Source);
                }
            }
        }
    }
}
