using Features.Abilities.Domain;
using Features.Buffs.UnityIntegration;
using Features.Combat.Devices;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Features.Abilities.UnityIntegration
{
    public sealed class ChargeDeviceHandler
        : AbilityHandler<ChargeDeviceAbilitySO>
    {
        protected override void ExecuteInternal(
            ChargeDeviceAbilitySO ability,
            AbilityContext ctx,
            GameObject owner)
        {
            if (ability.chargeFxPrefab == null || ability.areaBuff == null)
                return;

            var fx = Object.Instantiate(
                ability.chargeFxPrefab,
                owner.transform.position,
                Quaternion.identity
            );

            if (!fx.TryGetComponent(out NetworkObject fxNO))
            {
                Object.Destroy(fx);
                return;
            }

            var ownerNO = owner.GetComponent<NetworkObject>();

            InstanceFinder.ServerManager.Spawn(
                fxNO.gameObject,
                ownerNO != null ? ownerNO.Owner : null
            );

            float duration = ability.areaBuff.buff.duration;

            if (fx.TryGetComponent(out ChargeDeviceBehaviour beh))
                beh.Init(owner.transform, duration);

            if (fx.TryGetComponent(out AreaBuffEmitter emitter))
            {
                emitter.area = ability.areaBuff;
                emitter.enabled = true;
            }

            var auto =
                fx.GetComponent<NetworkAutoDespawn>()
                ?? fx.AddComponent<NetworkAutoDespawn>();

            auto.StartDespawn(duration + 0.25f);
        }
    }
}
