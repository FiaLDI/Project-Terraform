using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class StopContinuousEffect : IEffect
    {
        public void Apply(EffectContext context)
        {
            if (context.Source == null)
                return;


            ContinuousEffectRuntime.Instance.StopContinuous(
                context.Source
            );
        }
    }
}
