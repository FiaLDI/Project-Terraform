using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.Combat.Devices;

namespace Features.Abilities.UnityIntegration
{
    public class RepairDroneHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(RepairDroneAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (RepairDroneAbilitySO)abilityBase;

            // ==== ADAPTATION: ctx.Owner is NOW object ====
            GameObject ownerGO = null;

            switch (ctx.Owner)
            {
                case GameObject go:
                    ownerGO = go;
                    break;

                case Component comp:
                    ownerGO = comp.gameObject;
                    break;

                default:
                    Debug.LogError("[RepairDroneHandler] AbilityContext.Owner is not GameObject or Component.");
                    return;
            }

            if (ownerGO == null)
                return;

            if (!ability.dronePrefab)
            {
                Debug.LogError("[RepairDroneHandler] dronePrefab is null.");
                return;
            }

            GameObject droneObj = Object.Instantiate(
                ability.dronePrefab,
                ownerGO.transform.position,
                Quaternion.identity
            );

            // === Heal Aura Buff ===
            if (ability.healAura != null)
            {
                var emitter = droneObj.AddComponent<AreaBuffEmitter>();
                emitter.area = ability.healAura;
                Object.Destroy(emitter, ability.lifetime);
            }

            // === Behaviour Init ===
            if (droneObj.TryGetComponent<RepairDroneBehaviour>(out var drone))
            {
                drone.Init(ownerGO, ability.lifetime, ability.followSpeed);
            }

            Object.Destroy(droneObj, ability.lifetime + 0.3f);
        }
    }
}
