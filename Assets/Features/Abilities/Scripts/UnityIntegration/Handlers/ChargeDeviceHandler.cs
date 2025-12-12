using UnityEngine;
using Features.Abilities.Domain;
using Features.Buffs.UnityIntegration;
using Features.Combat.Devices;

namespace Features.Abilities.UnityIntegration
{
    public class ChargeDeviceHandler : IAbilityHandler
    {
        public System.Type AbilityType => typeof(ChargeDeviceAbilitySO);

        public void Execute(AbilitySO abilityBase, AbilityContext ctx)
        {
            var ability = (ChargeDeviceAbilitySO)abilityBase;

            // ================================
            // ADAPTATION: ctx.Owner is NOW object — not GameObject
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

            // ================================
            // BUFF DURATION
            // ================================
            float duration = 0f;

            if (ability.areaBuff != null && ability.areaBuff.buff != null)
                duration = ability.areaBuff.buff.duration;

            // ================================
            // APPLY AREA BUFF ON OWNER
            // ================================
            if (ability.areaBuff != null)
            {
                var emitter = ownerGO.AddComponent<AreaBuffEmitter>();
                emitter.area = ability.areaBuff;

                if (duration > 0f)
                    Object.Destroy(emitter, duration);
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
