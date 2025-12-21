using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;
using Features.Player.UnityIntegration;
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

            // Add missing components
            if (!turret.TryGetComponent(out BuffSystem bs))
                turret.AddComponent<BuffSystem>();

            if (!turret.TryGetComponent<IBuffTarget>(out _))
                turret.AddComponent<TurretBuffTarget>();

            // Register turret
            PlayerDeviceRegistry.Instance?.RegisterDevice(owner, turret);

            // Smooth spawn animation
            turret.transform.localScale = Vector3.zero;
            turret.AddComponent<TurretSpawnAnimation>();

            Object.Destroy(turret, ability.duration + 0.1f);
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

                // Step 1 — Raycast to ground
                if (!Physics.Raycast(horizontalPoint + Vector3.up * 3f, Vector3.down,
                                    out var hit, 10f, groundMask))
                    continue;

                Vector3 groundPos = hit.point;

                // Step 2 — Check slope
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > groundSlopeLimit)
                    continue;

                // Step 3 — Prevent spawning inside walls or obstacles
                if (Physics.CheckSphere(groundPos + Vector3.up * 0.5f, turretRadius, obstacleMask))
                    continue;

                // Step 4 — Prevent spawning too close to another turret
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

            // Fallback: the player’s position (offset up)
            return origin + Vector3.up * 0.2f;
        }

    }

}
