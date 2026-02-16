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

        public SpawnProjectileEffect(ProjectileConfig config)
        {
            _config = config;
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

            var ownerNetObj =
                context.Source is Component c
                    ? c.GetComponentInParent<NetworkObject>()
                    : null;

            InstanceFinder.ServerManager.Spawn(go, ownerNetObj?.Owner);

            var projectile = go.GetComponent<ProjectileNetwork>();
            projectile.InitServer(_config, ownerNetObj);
        }
    }
}
