using Features.Effects.Domain;
using Features.Buffs.Domain;

namespace Features.Effects.Application
{
    public sealed class HealInstantEffect : IEffect
    {
        private readonly float _value;

        public HealInstantEffect(float value)
        {
            _value = value;
        }

        public void Apply(EffectContext context)
        {
            if (context.Targets == null)
                return;

            foreach (var target in context.Targets)
            {
                var stats = target.GetServerStats();
                if (stats?.Health == null)
                    continue;

                stats.Health.Heal(_value);
            }
        }
    }
}
