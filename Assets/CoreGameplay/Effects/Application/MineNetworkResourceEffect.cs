using UnityEngine;
using FishNet;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Effects.Application
{
    public sealed class MineNetworkResourceEffect : IEffect
    {
        private readonly float _value;
        private readonly float _range;
        private readonly LayerMask _mask;

        public MineNetworkResourceEffect(
            float value,
            float range,
            LayerMask mask)
        {
            _value = value;
            _range = range;
            _mask = mask;
        }

        public void Apply(EffectContext context)
        {
            if (!InstanceFinder.IsServer)
                return;

            Debug.DrawRay(
                context.Origin,
                context.Direction * _range,
                Color.green,
                1.5f
            );

            bool hitSomething = Physics.Raycast(
                context.Origin,
                context.Direction,
                out var hit,
                _range,
                _mask);

            if (!hitSomething)
            {
                return;
            }

            if (hit.collider.TryGetComponent<ResourceNodeNetwork>(out var node))
            {
                node.Mine_Server(_value, 1f);
            }
            else
            {

            }
        }
    }
}