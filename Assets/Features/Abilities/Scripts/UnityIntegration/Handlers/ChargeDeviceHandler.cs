using UnityEngine;
using Features.Abilities.Domain;
using Features.Combat.Devices;
using FishNet.Object;
using FishNet.Managing;
using FishNet;
using System.Collections;

namespace Features.Abilities.UnityIntegration
{
    public sealed class ChargeDeviceHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(ChargeDeviceAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (ChargeDeviceAbilitySO)abilityBase;

            if (!TryResolveOwner(ctx.Owner, out var ownerGO))
                return;

            var ownerNO = ownerGO.GetComponent<NetworkObject>();
            if (ownerNO == null || !ownerNO.IsServer)
                return; // 🔒 только сервер

            if (ability.chargeFxPrefab == null || ability.areaBuff == null)
                return;

            var prefabNO = ability.chargeFxPrefab.GetComponent<NetworkObject>();
            if (prefabNO == null)
            {
                Debug.LogError("[ChargeDeviceHandler] chargeFxPrefab has no NetworkObject");
                return;
            }

            float duration = ability.areaBuff.buff.duration;

            // 🔥 SERVER SPAWN
            var fx = Object.Instantiate(
                ability.chargeFxPrefab,
                ownerGO.transform.position,
                Quaternion.identity
            );

            var fxNO = fx.GetComponent<NetworkObject>();
            InstanceFinder.ServerManager.Spawn(fxNO);

            // follow + lifetime
            if (fx.TryGetComponent(out ChargeDeviceBehaviour beh))
                beh.Init(ownerGO.transform, duration);

            // 🔥 AURA (уже на активном NetworkObject)
            if (fx.TryGetComponent(out Features.Buffs.UnityIntegration.AreaBuffEmitter emitter))
            {
                emitter.area = ability.areaBuff;
                emitter.enabled = true;
            }


            if (fx.TryGetComponent(out NetworkAutoDespawn auto))
            {
                auto.StartDespawn(duration + 0.25f);
            }
            else
            {
                auto = fx.AddComponent<NetworkAutoDespawn>();
                auto.StartDespawn(duration + 0.25f);
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
                Debug.LogError("[ChargeDeviceHandler] Invalid AbilityContext.Owner");
                return false;
            }

            return true;
        }
    }
}
