using Features.Abilities.Domain;
using Features.Buffs.UnityIntegration;
using Features.Combat.Devices;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public sealed class RepairDroneHandler
        : AbilityHandler<RepairDroneAbilitySO>
    {
        protected override void ExecuteInternal(
            RepairDroneAbilitySO ability,
            AbilityContext ctx,
            GameObject owner)
        {
            if (!ability.dronePrefab)
                return;

            var drone = Object.Instantiate(
                ability.dronePrefab,
                owner.transform.position,
                Quaternion.identity
            );

            if (!drone.TryGetComponent(out NetworkObject droneNO))
            {
                Debug.LogError(
                    "[RepairDroneHandler] dronePrefab has no NetworkObject"
                );
                Object.Destroy(drone);
                return;
            }

            var ownerNO = owner.GetComponent<NetworkObject>();

            InstanceFinder.ServerManager.Spawn(
                droneNO.gameObject,
                ownerNO != null ? ownerNO.Owner : null
            );

            // ===== HEAL AURA =====
            if (ability.healAura != null)
            {
                var emitter =
                    drone.GetComponent<AreaBuffEmitter>()
                    ?? drone.AddComponent<AreaBuffEmitter>();

                emitter.area = ability.healAura;
                emitter.enabled = true;
            }

            // ===== BEHAVIOUR =====
            if (drone.TryGetComponent(out RepairDroneBehaviour beh))
            {
                beh.Init(
                    owner,
                    ability.lifetime,
                    ability.followSpeed
                );
            }

            // ===== AUTO DESPAWN =====
            var auto =
                drone.GetComponent<NetworkAutoDespawn>()
                ?? drone.AddComponent<NetworkAutoDespawn>();

            auto.StartDespawn(ability.lifetime + 0.25f);
        }
    }
}
