using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class ContinuousEffect : IEffect
    {
        private readonly float _interval;
        private readonly EffectDefinition[] _child;

        public ContinuousEffect(
            float interval,
            EffectDefinition[] child)
        {
            _interval = interval;
            _child = child;
        }

        public void Apply(EffectContext context)
        {
            ContinuousEffectRuntime.Instance.StartContinuous(
                context.Source,
                _interval,
                _child,
                context
            );
        }
    }
}
