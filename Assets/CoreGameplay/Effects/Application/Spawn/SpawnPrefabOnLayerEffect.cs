using UnityEngine;
using Features.Effects.Domain;
using Features.Buffs.Domain;
using FishNet.Object;


namespace Features.Effects.Application
{
    public sealed class SpawnPrefabOnLayerEffect : IEffect
    {
        private readonly string _prefabId;
        private readonly float _lifetime;
        private readonly float _heightOffset;

        public SpawnPrefabOnLayerEffect(
            string prefabId,
            float lifetime,
            float heightOffset)
        {
            _prefabId = prefabId;
            _lifetime = lifetime;
            _heightOffset = heightOffset;
        }

        public void Apply(EffectContext context)
        {
            if (context.Targets == null || context.Targets.Length == 0)
                return;

            if (SpawnService.Instance == null)
                return;

            foreach (var target in context.Targets)
            {
                if (target is not Component comp)
                    continue;

                if (!comp.TryGetComponent<NetworkObject>(out _))
                    continue;

                Vector3 pos =
                    comp.transform.position + Vector3.up * _heightOffset;

                SpawnService.Instance.SpawnAtPosition(
                    _prefabId,
                    pos,
                    Quaternion.identity,
                    _lifetime
                );
            }
        }
    }
}