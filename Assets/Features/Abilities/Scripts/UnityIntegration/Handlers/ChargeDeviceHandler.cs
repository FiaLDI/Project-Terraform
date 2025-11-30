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
            var owner = ctx.Owner;
            if (!owner) return;

            float duration = ability.areaBuff != null && ability.areaBuff.buff != null
                ? ability.areaBuff.buff.duration
                : 0f;

            if (ability.areaBuff != null)
            {
                var emitter = owner.AddComponent<AreaBuffEmitter>();
                emitter.area = ability.areaBuff;
                Object.Destroy(emitter, duration);
            }

            // FX
            if (ability.chargeFxPrefab)
            {
                GameObject fx = Object.Instantiate(
                    ability.chargeFxPrefab,
                    owner.transform.position,
                    Quaternion.identity
                );

                if (fx.TryGetComponent<ChargeDeviceBehaviour>(out var beh))
                    beh.Init(owner.transform, duration);

                Object.Destroy(fx, duration + 0.2f);
            }
        }
    }
}
