using Features.Effects.Domain;
using Features.Resources.Domain;

namespace Features.Effects.Application
{
    public sealed class ScanEffect : IEffect
    {
        private readonly float _strength;

        public ScanEffect(float strength)
        {
            _strength = strength;
        }

        public void Apply(EffectContext context)
        {
            if (context.Targets == null)
                return;

            foreach (var t in context.Targets)
            {
                if (t is IScannable scannable)
                    scannable.OnScanned(_strength);
            }
        }
    }
}
