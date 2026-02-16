using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Stats.Domain;

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

                var statsOwner = t.BuffSystem.GetComponent<IStatsOwner>();
                if (statsOwner == null || !statsOwner.IsReady)
                    continue;

                var stats = statsOwner.Facade;
                stats?.Health?.Damage(_value);
            }
        }
    }
}
