using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public sealed class DeployTurretHandler
        : AbilityHandler<DeployTurretAbilitySO>
    {
        private const float SEARCH_RADIUS = 2.2f;
        private const int SEARCH_POINTS = 6;
        private const float SLOPE_LIMIT = 22f;
        private const float TURRET_RADIUS = 0.5f;
        private const float MIN_TURRET_SPACING = 1.5f;

        protected override void ExecuteInternal(
            DeployTurretAbilitySO ability,
            AbilityContext ctx,
            GameObject owner)
        {
            if (!owner.TryGetComponent(out NetworkObject ownerNO))
                return;

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

            var turret = Object.Instantiate(
                ability.turretPrefab,
                spawnPos,
                Quaternion.identity
            );

            if (!turret.TryGetComponent(out NetworkObject turretNO))
            {
                Object.Destroy(turret);
                return;
            }

            InstanceFinder.ServerManager.Spawn(
                turretNO.gameObject,
                ownerNO.Owner
            );

            // ===== BUFF INFRA =====
            if (!turret.TryGetComponent<IBuffTarget>(out _))
                turret.AddComponent<TurretBuffTarget>();

            if (!turret.TryGetComponent<BuffSystem>(out _))
                turret.AddComponent<BuffSystem>();

            PlayerDeviceRegistry.Instance?.RegisterDevice(owner, turret);

            // ===== VISUAL =====
            turret.transform.localScale = Vector3.zero;
            turret.AddComponent<TurretSpawnAnimation>();

            // ===== LIFETIME =====
            if (turret.TryGetComponent<TurretBehaviour>(out var beh))
                beh.ScheduleDestruction(ability.duration);
        }

        private static Vector3 FindBestTurretSpawnPoint(
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
                Vector3 dir = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 horizontalPoint = origin + dir * searchRadius;

                if (!Physics.Raycast(
                        horizontalPoint + Vector3.up * 3f,
                        Vector3.down,
                        out var hit,
                        10f,
                        groundMask))
                    continue;

                if (Vector3.Angle(hit.normal, Vector3.up) > groundSlopeLimit)
                    continue;

                Vector3 groundPos = hit.point;

                if (Physics.CheckSphere(
                        groundPos + Vector3.up * 0.5f,
                        turretRadius,
                        obstacleMask))
                    continue;

                foreach (var c in Physics.OverlapSphere(groundPos, spacingBetweenTurrets))
                {
                    if (c.GetComponent<TurretBehaviour>())
                        goto Skip;
                }

                return groundPos;

            Skip:
                continue;
            }

            return origin + Vector3.up * 0.2f;
        }
    }
}
