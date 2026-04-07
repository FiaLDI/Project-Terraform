using UnityEngine;
using FishNet;
using Features.Effects.Domain;

namespace Features.Effects.Application
{
    public sealed class SpawnImpactEffect : IEffect
    {
        private readonly string _fxId;

        public SpawnImpactEffect(string fxId)
        {
            _fxId = fxId;
        }

        public void Apply(EffectContext context)
        {
            if (!InstanceFinder.IsServer)
                return;

            if (string.IsNullOrEmpty(_fxId))
                return;

            ImpactFxDispatcher.Instance.ServerSpawn(
                context.Origin,
                context.Direction,
                _fxId
            );
        }
    }
}
