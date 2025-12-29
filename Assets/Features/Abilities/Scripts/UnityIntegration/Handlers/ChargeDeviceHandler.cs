using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.Application;
using Features.Combat.Devices;
using FishNet.Object;

namespace Features.Abilities.UnityIntegration
{
    public class ChargeDeviceHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(ChargeDeviceAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (ChargeDeviceAbilitySO)abilityBase;

            // ================================
            // ADAPTATION: ctx.Owner is NOW object – not GameObject
            // ================================
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
                    Debug.LogError("[ChargeDeviceHandler] AbilityContext.Owner is not GameObject or Component.");
                    return;
            }

            if (ownerGO == null)
                return;

            // ✅ ТОЛЬКО НА СЕРВЕРЕ - предотвращаем двойное применение на клиенте
            var netObj = ownerGO.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsServer)
            {
                // На клиенте: баффы синхронизируются через SyncList от сервера
                return;
            }

            // ================================
            // BUFF DURATION
            // ================================
            float duration = 0f;

            if (ability.areaBuff != null && ability.areaBuff.buff != null)
                duration = ability.areaBuff.buff.duration;

            // ================================
            // APPLY BUFF DIRECTLY TO OWNER (ТОЛЬКО НА СЕРВЕРЕ)
            // ================================
            if (ability.areaBuff != null && ability.areaBuff.buff != null)
            {
                var buffSystem = ownerGO.GetComponent<BuffSystem>();
                if (buffSystem != null)
                {
                    // ✅ Добавляем бафф ТОЛЬКО владельцу (только на сервере!)
                    buffSystem.Add(ability.areaBuff.buff);
                    
                    Debug.Log($"[ChargeDeviceHandler] Buff '{ability.areaBuff.buff.buffId}' applied to owner: {ownerGO.name}");
                }
                else
                {
                    Debug.LogWarning($"[ChargeDeviceHandler] Owner {ownerGO.name} has no BuffSystem component");
                }
            }

            // ================================
            // FX
            // ================================
            if (ability.chargeFxPrefab)
            {
                GameObject fx = Object.Instantiate(
                    ability.chargeFxPrefab,
                    ownerGO.transform.position,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<ChargeDeviceBehaviour>(out var beh))
                    beh.Init(ownerGO.transform, duration);

                Object.Destroy(fx, duration + 0.2f);
            }
        }
    }
}