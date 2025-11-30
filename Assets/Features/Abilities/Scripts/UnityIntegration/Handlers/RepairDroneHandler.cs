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
            var owner = ctx.Owner;
            if (!owner) return;

            var buffs = owner.GetComponent<BuffSystem>();

            if (!ability.dronePrefab)
            {
                Debug.LogError("[RepairDroneHandler] dronePrefab is null.");
                return;
            }

            GameObject droneObj = Object.Instantiate(
                ability.dronePrefab,
                owner.transform.position,
                Quaternion.identity
            );

            if (ability.healAura != null)
            {
                var emitter = droneObj.AddComponent<AreaBuffEmitter>();
                emitter.area = ability.healAura;
                Object.Destroy(emitter, ability.lifetime);
            }

            if (droneObj.TryGetComponent<RepairDroneBehaviour>(out var drone))
            {
                drone.Init(owner, ability.lifetime, ability.followSpeed);
            }

            Object.Destroy(droneObj, ability.lifetime + 0.3f);
        }
    }
}
