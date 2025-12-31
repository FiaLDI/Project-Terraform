using UnityEngine;
using Features.Abilities.Domain;
using Features.Combat.Devices;
using FishNet.Object;

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

            var netObj = ownerGO.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
                return;

            if (ability.chargeFxPrefab == null || ability.areaBuff == null)
                return;

            float duration = ability.areaBuff.buff.duration;

            // 🟢 СПАВНИМ FX + AURA
            var fx = Object.Instantiate(
                ability.chargeFxPrefab,
                ownerGO.transform.position,
                Quaternion.identity
            );

            // lifetime + follow
            if (fx.TryGetComponent(out ChargeDeviceBehaviour beh))
                beh.Init(ownerGO.transform, duration);

            // на префабе УЖЕ должен быть AreaBuffEmitter
            if (fx.TryGetComponent(out Features.Buffs.UnityIntegration.AreaBuffEmitter emitter))
            {
                emitter.area = ability.areaBuff;
            }

            Object.Destroy(fx, duration + 0.2f);
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
