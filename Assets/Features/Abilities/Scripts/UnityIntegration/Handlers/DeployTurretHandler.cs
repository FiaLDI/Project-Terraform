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
                Debug.LogWarning("[DeployTurretHandler] turretPrefab is null.");
                return;
            }

            GameObject obj = Object.Instantiate(
                ability.turretPrefab,
                ctx.TargetPoint,
                Quaternion.identity
            );

            if (obj.TryGetComponent<TurretBehaviour>(out var turret))
            {
                turret.Init(
                    ctx.Owner,
                    ability.hp,
                    ability.damagePerSecond,
                    ability.range,
                    ability.duration
                );
            }

            Object.Destroy(obj, ability.duration + 0.3f);
        }
    }
}
