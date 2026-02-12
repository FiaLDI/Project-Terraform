using UnityEngine;
using FishNet;
using FishNet.Object;
using Features.Effects.Domain;
using Features.Weapons.Domain;

namespace Features.Effects.Application
{
    public sealed class SpawnProjectileEffect : IEffect
    {
        private readonly ProjectileConfig _config;
        private readonly NetworkObject _owner;

        public SpawnProjectileEffect(
            ProjectileConfig config,
            NetworkObject owner)
        {
            _config = config;
            _owner = owner;
        }

        public void Apply(EffectContext context)
        {
            if (!InstanceFinder.IsServer)
                return;

            if (_config == null)
                return;

            var go = Object.Instantiate(
                _config.projectilePrefab,
                context.Origin,
                Quaternion.LookRotation(context.Direction)
            );

            var net = go.GetComponent<NetworkObject>();
            InstanceFinder.ServerManager.Spawn(go, _owner?.Owner);

            var projectile = go.GetComponent<ProjectileNetwork>();
            projectile.InitServer(_config, _owner);
        }
    }
}
