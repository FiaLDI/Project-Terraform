using FishNet.Object;
using UnityEngine;
using Features.Equipment.UnityIntegration;
using Features.Weapons.UnityIntegration;

namespace Features.Weapons.UnityIntegration
{
    [RequireComponent(typeof(EquipmentManager))]
    public sealed class WeaponFxNetwork : NetworkBehaviour
    {
        private EquipmentManager equipment;

        private void Awake()
        {
            equipment = GetComponent<EquipmentManager>();
        }

        [Server]
        public void NotifyFire()
        {
            RpcPlayFireFx();
        }

        [Server]
        public void NotifyReload(bool emptyReload)
        {
            RpcPlayReloadFx(emptyReload);
        }

        [ObserversRpc]
        private void RpcPlayFireFx()
        {
            FindRightHandWeapon()?.PlayFireFxClient();
        }

        [ObserversRpc]
        private void RpcPlayReloadFx(bool emptyReload)
        {
            FindRightHandWeapon()?.PlayReloadFxClient(emptyReload);
        }

        private WeaponController FindRightHandWeapon()
        {
            var usable = equipment?.GetRightHandUsable();
            if (usable is Component c)
                return c.GetComponent<WeaponController>()
                    ?? c.GetComponentInChildren<WeaponController>();

            return null;
        }
    }
}
