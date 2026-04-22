using UnityEngine;
using FishNet;
using Features.Effects.Domain;
using Features.Weapons.Domain;
using Features.Buffs.Domain;
using FishNet.Object;

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
            if (!InstanceFinder.IsServer || _config == null)
                return;

            Vector3 origin = context.Origin;
            Vector3 dir = context.Direction.normalized;

            if (_config.useServerProjectile)
            {
                SpawnServerProjectile(context, origin, dir);
                return;
            }

            // ===============================
            // HITSCAN
            // ===============================

            Vector3 hitPoint = origin + dir * 100f;
            Vector3 normal = -dir;
            IBuffTarget target = null;

            if (Physics.Raycast(origin, dir, out var hit, 1000f))
            {
                hitPoint = hit.point;
                normal = hit.normal;

                target =
                    hit.collider.GetComponent<IBuffTarget>() ??
                    hit.collider.GetComponentInParent<IBuffTarget>();
            }

            // ===============================
            // DAMAGE
            // ===============================

            if (target != null)
            {
                var ctx = new EffectContext(
                    context.Source,
                    new[] { target },
                    hitPoint,
                    dir
                );

                EffectExecutor.Instance.Execute(
                    new EffectDefinition
                    {
                        type = EffectType.DealDamage,
                        value = _config.damage,
                        targetMode = TargetMode.Explicit
                    },
                    ctx
                );
            }

            // ===============================
            // VISUAL
            // ===============================

            var adapter = (context.Source as Component)
                ?.GetComponentInParent<PlayerUsageNetAdapter>();

            if (adapter != null)
                adapter.ServerNotifyShot(origin, hitPoint);
        }

        private void SpawnServerProjectile(
            EffectContext context,
            Vector3 origin,
            Vector3 dir)
        {
            if (_config.projectilePrefab == null)
            {
                Debug.LogError("[SpawnProjectileEffect] Server projectile prefab is null.");
                return;
            }

            var go = Object.Instantiate(
                _config.projectilePrefab,
                origin,
                Quaternion.LookRotation(dir)
            );

            var net = go.GetComponent<NetworkObject>();
            if (net == null)
            {
                Debug.LogError("[SpawnProjectileEffect] Server projectile prefab has no NetworkObject.", go);
                Object.Destroy(go);
                return;
            }

            var owner = (context.Source as Component)
                ?.GetComponentInParent<NetworkObject>();

            InstanceFinder.ServerManager.Spawn(net, owner?.Owner);

            var proj = go.GetComponent<ProjectileNetwork>();
            if (proj == null)
            {
                Debug.LogError("[SpawnProjectileEffect] Server projectile prefab has no ProjectileNetwork.", go);
                net.Despawn();
                return;
            }

            proj.InitServer(_config, owner, dir);
        }
    }
}
