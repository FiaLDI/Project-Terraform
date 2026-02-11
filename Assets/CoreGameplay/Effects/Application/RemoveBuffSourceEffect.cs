using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class RemoveBuffSourceEffect : IEffect
    {
        private readonly bool _onlySpecific;
        private readonly string _buffId;

        public RemoveBuffSourceEffect(bool onlySpecific, string buffId)
        {
            _onlySpecific = onlySpecific;
            _buffId = buffId;
        }

        public void Apply(EffectContext context)
        {
            if (context.Targets == null)
                return;

            foreach (var target in context.Targets)
            {
                if (target is not IBuffTarget buffTarget)
                    continue;

                var system = buffTarget.BuffSystem;
                if (system == null)
                    continue;

                if (!_onlySpecific)
                {
                    system.RemoveBySource(context.Source);
                }
                else
                {
                    system.RemoveBySourceAndId(context.Source, _buffId);
                }
            }
        }
    }
}
