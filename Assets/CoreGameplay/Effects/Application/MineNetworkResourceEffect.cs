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

            Debug.Log("=== MINE TICK ===");
            Debug.Log($"Origin: {context.Origin}");
            Debug.Log($"Direction: {context.Direction}");
            Debug.Log($"Range: {_range}");
            Debug.Log($"Mask value: {_mask.value}");

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

            Debug.Log($"Raycast result: {hitSomething}");

            if (!hitSomething)
            {
                Debug.Log("SERVER RAY MISS");
                return;
            }

            Debug.Log($"Hit object: {hit.collider.name}");
            Debug.Log($"Hit layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            Debug.Log($"Hit distance: {hit.distance}");

            if (hit.collider.TryGetComponent<ResourceNodeNetwork>(out var node))
            {
                Debug.Log("RESOURCE NODE FOUND → Mining");
                node.Mine_Server(_value, 1f);
            }
            else
            {
                Debug.Log("Hit but no ResourceNodeNetwork component");
            }
        }
    }
}