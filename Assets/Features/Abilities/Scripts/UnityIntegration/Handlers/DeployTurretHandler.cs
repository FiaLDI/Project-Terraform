using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;
using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public class DeployTurretHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(DeployTurretAbilitySO);

        private const float SEARCH_RADIUS = 2.2f;
        private const int SEARCH_POINTS = 6;
        private const float SLOPE_LIMIT = 22f;
        private const float TURRET_RADIUS = 0.5f;
        private const float MIN_TURRET_SPACING = 1.5f;

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (DeployTurretAbilitySO)abilityBase;

            GameObject owner = ctx.Owner as GameObject 
                            ?? (ctx.Owner as Component)?.gameObject;

            if (!owner) return;

            // 🎯 Получаем NetworkObject и ServerManager прямо здесь
            if (!owner.TryGetComponent<NetworkObject>(out var ownerNetObj))
            {
                Debug.LogError("[DeployTurretHandler] Owner has no NetworkObject!");
                return;
            }

            if (!ownerNetObj.IsServer)
            {
                Debug.LogError("[DeployTurretHandler] Only server can spawn turrets!");
                return;
            }

            var serverManager = ownerNetObj.ServerManager;
            if (serverManager == null)
            {
                Debug.LogError("[DeployTurretHandler] ServerManager not available!");
                return;
            }

            Vector3 spawnPos = FindBestTurretSpawnPoint(
                owner.transform,
                SEARCH_RADIUS,
                SEARCH_POINTS,
                SLOPE_LIMIT,
                MIN_TURRET_SPACING,
                TURRET_RADIUS,
                LayerMask.GetMask("Ground"),
                LayerMask.GetMask("Default", "Environment")
            );

            GameObject turret = Object.Instantiate(
                ability.turretPrefab,
                spawnPos,
                Quaternion.identity
            );

            if (turret.TryGetComponent<NetworkObject>(out var turretNetObj))
            {
                // 🎯 Спавним турель через ServerManager
                serverManager.Spawn(turretNetObj.gameObject, ownerNetObj.Owner);
            }
            else
            {
                Debug.LogError("[DeployTurretHandler] Turret prefab has no NetworkObject!", turret);
                Object.Destroy(turret);
                return;
            }

            if (!turret.TryGetComponent(out BuffSystem bs))
                turret.AddComponent<BuffSystem>();

            if (!turret.TryGetComponent<IBuffTarget>(out _))
                turret.AddComponent<TurretBuffTarget>();

            PlayerDeviceRegistry.Instance?.RegisterDevice(owner, turret);

            turret.transform.localScale = Vector3.zero;
            turret.AddComponent<TurretSpawnAnimation>();

            if (turret.TryGetComponent<TurretBehaviour>(out var turretBehaviour))
            {
                turretBehaviour.ScheduleDestruction(ability.duration);
            }
        }

        private Vector3 FindBestTurretSpawnPoint(
            Transform player,
            float searchRadius,
            int pointCount,
            float groundSlopeLimit,
            float spacingBetweenTurrets,
            float turretRadius,
            LayerMask groundMask,
            LayerMask obstacleMask)
        {
            Vector3 origin = player.position;
            float angleStep = 360f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 horizontalPoint = origin + dir * searchRadius;

                if (!Physics.Raycast(horizontalPoint + Vector3.up * 3f, Vector3.down,
                                    out var hit, 10f, groundMask))
                    continue;

                Vector3 groundPos = hit.point;

                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > groundSlopeLimit)
                    continue;

                if (Physics.CheckSphere(groundPos + Vector3.up * 0.5f, turretRadius, obstacleMask))
                    continue;

                Collider[] nearTurrets = Physics.OverlapSphere(groundPos, spacingBetweenTurrets);
                foreach (var c in nearTurrets)
                {
                    if (c.GetComponent<TurretBehaviour>())
                    {
                        goto SkipThisPoint;
                    }
                }

                return groundPos;

            SkipThisPoint:
                continue;
            }

            return origin + Vector3.up * 0.2f;
        }
    }
}
