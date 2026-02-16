using UnityEngine;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Combat.Domain;

namespace Features.Effects.Application
{
    public sealed class HitscanDamageEffect : IEffect
    {
        private readonly float _damage;
        private readonly float _range;
        private readonly LayerMask _mask;
        private readonly DamageType _type;

        public HitscanDamageEffect(
            float damage,
            float range,
            LayerMask mask,
            DamageType type)
        {
            _damage = damage;
            _range = range;
            _mask = mask;
            _type = type;
        }

        public void Apply(EffectContext context)
        {
            if (!Physics.Raycast(
                    context.Origin,
                    context.Direction,
                    out var hit,
                    _range,
                    _mask))
                return;

            if (!hit.collider.TryGetComponent<IBuffTarget>(out var target))
                return;

            var newCtx = new EffectContext(
                context.Source,
                new[] { target },
                hit.point,
                context.Direction
            );

            new DealDamageEffect(_damage, _type).Apply(newCtx);
        }
    }
}
