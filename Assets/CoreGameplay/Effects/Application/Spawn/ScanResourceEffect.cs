using UnityEngine;
using FishNet;
using FishNet.Object;
using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class ScanResourceEffect : IEffect
    {
        private readonly string _prefabId;
        private readonly float _radius;
        private readonly LayerMask _mask;
        private readonly float _lifetime;
        private readonly float _heightOffset;

        public ScanResourceEffect(
            string prefabId,
            float radius,
            LayerMask mask,
            float lifetime,
            float heightOffset)
        {
            _prefabId = prefabId;
            _radius = radius;
            _mask = mask;
            _lifetime = lifetime;
            _heightOffset = heightOffset;
        }



        public void Apply(EffectContext context)
        {
            if (!InstanceFinder.IsServer)
                return;

            var hits = Physics.OverlapSphere(
                context.Origin,
                _radius,
                _mask);

            Debug.Log($"[SCAN] Found {hits.Length} objects");

            foreach (var col in hits)
            {
                if (!col.TryGetComponent<ResourceNodeNetwork>(out var node))
                    continue;

                Vector3 spawnPos =
                    node.transform.position + Vector3.up * _heightOffset;

                SpawnService.Instance.SpawnAtPosition(
                    _prefabId,
                    spawnPos,
                    Quaternion.identity,
                    _lifetime
                );
            }
        }
    }
}