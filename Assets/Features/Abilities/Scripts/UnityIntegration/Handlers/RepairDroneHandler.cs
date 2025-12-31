using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.UnityIntegration;
using Features.Combat.Devices;
using FishNet.Object;
using FishNet.Managing;
using FishNet;

namespace Features.Abilities.UnityIntegration
{
    public sealed class RepairDroneHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(RepairDroneAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (RepairDroneAbilitySO)abilityBase;

            if (!TryResolveOwner(ctx.Owner, out var ownerGO))
                return;

            var ownerNO = ownerGO.GetComponent<NetworkObject>();
            if (ownerNO == null || !ownerNO.IsServer)
                return;

            if (!ability.dronePrefab)
                return;

            var prefabNO = ability.dronePrefab.GetComponent<NetworkObject>();
            if (prefabNO == null)
            {
                Debug.LogError("[RepairDroneHandler] dronePrefab has no NetworkObject");
                return;
            }

            // 🔥 SERVER SPAWN
            var drone = Object.Instantiate(
                ability.dronePrefab,
                ownerGO.transform.position,
                Quaternion.identity
            );

            var droneNO = drone.GetComponent<NetworkObject>();
            InstanceFinder.ServerManager.Spawn(droneNO);

            // === Heal Aura ===
            if (ability.healAura != null)
            {
                var emitter = drone.GetComponent<AreaBuffEmitter>();
                if (emitter == null)
                    emitter = drone.AddComponent<AreaBuffEmitter>();

                emitter.area = ability.healAura;
                emitter.enabled = true;
            }

            // === Behaviour ===
            if (drone.TryGetComponent(out RepairDroneBehaviour beh))
            {
                beh.Init(ownerGO, ability.lifetime, ability.followSpeed);
            }

            if (drone.TryGetComponent(out NetworkAutoDespawn auto))
            {
                auto.StartDespawn(ability.lifetime + 0.25f);
            }
            else
            {
                auto = drone.AddComponent<NetworkAutoDespawn>();
                auto.StartDespawn(ability.lifetime + 0.25f);
            }
        }

        private bool TryResolveOwner(object owner, out GameObject go)
        {
            go = owner switch
            {
                GameObject g => g,
                Component c => c.gameObject,
                _ => null
            };

            if (go == null)
            {
                Debug.LogError("[RepairDroneHandler] Invalid AbilityContext.Owner");
                return false;
            }

            return true;
        }
    }
}
