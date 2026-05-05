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
    private static readonly RaycastHit[] HitscanHits = new RaycastHit[32];
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
            var owner = (context.Source as Component)
                ?.GetComponentInParent<NetworkObject>();

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                dir,
                HitscanHits,
                1000f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            if (TryResolveHitscanHit(owner, hitCount, out var hit))
            {
                hitPoint = hit.point;
                normal = hit.normal;

                if ((_config.hitMask.value & (1 << hit.collider.gameObject.layer)) != 0)
                {
                    target =
                        hit.collider.GetComponentInParent<StatsBuffTarget>() as IBuffTarget ??
                        hit.collider.GetComponentInParent<ResourceNodeNetwork>() as IBuffTarget ??
                        hit.collider.GetComponent<IBuffTarget>() ??
                        hit.collider.GetComponentInParent<IBuffTarget>();
                }
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
                        targetMode = TargetMode.Explicit,
                        damageType = _config.damageType
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

        private static bool TryResolveHitscanHit(
            NetworkObject owner,
            int hitCount,
            out RaycastHit bestHit)
        {
            bestHit = default;
            if (hitCount <= 0)
                return false;

            System.Array.Sort(HitscanHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = HitscanHits[i];
                if (hit.collider == null)
                    continue;

                if (IsOwnerCollider(owner, hit.collider))
                    continue;

                bestHit = hit;
                return true;
            }

            return false;
        }

        private static bool IsOwnerCollider(NetworkObject owner, Collider collider)
        {
            if (owner == null || collider == null)
                return false;

            var otherNetworkObject = collider.GetComponentInParent<NetworkObject>();
            if (otherNetworkObject != null && otherNetworkObject == owner)
                return true;

            return collider.transform == owner.transform ||
                   collider.transform.IsChildOf(owner.transform);
        }

        private sealed class RaycastHitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly RaycastHitDistanceComparer Instance = new();

            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
