using UnityEngine;
using Features.Abilities.Domain;
using Features.Combat.Devices;

namespace Features.Abilities.UnityIntegration
{
    public class DeployTurretHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(DeployTurretAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (DeployTurretAbilitySO)abilityBase;

            if (!ability.turretPrefab)
            {
                Debug.LogWarning("[DeployTurretHandler] TurretPrefab is null.");
                return;
            }

            // Spawn
            GameObject obj = Object.Instantiate(
                ability.turretPrefab,
                ctx.TargetPoint,
                Quaternion.identity
            );

            // Register turret to player
            if (PlayerRegistry.Instance != null)
            {
                PlayerRegistry.Instance.RegisterTurret(ctx.Owner, obj);
            }

            // Destroy after ability duration
            Object.Destroy(obj, ability.duration + 0.1f);
        }
    }
}
