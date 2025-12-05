using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Buffs.UnityIntegration;

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

            // Ensure buff system
            if (!obj.TryGetComponent<BuffSystem>(out var bs))
                bs = obj.AddComponent<BuffSystem>();

            // Ensure buff target
            if (!obj.TryGetComponent<IBuffTarget>(out _))
                obj.AddComponent<TurretBuffTarget>();

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
