using Features.Items.Domain;
using Features.Weapons.Data;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public class ReloadService
    {
        private readonly AmmoService ammoService;

        public ReloadService(AmmoService ammoService)
        {
            this.ammoService = ammoService;
        }

        public bool CanReload(ItemInstance inst, WeaponConfig config, WeaponAmmoState ammo)
        {
            if (ammo.isReloading) return false;
            if (ammo.ammoInMagazine >= config.magazineSize) return false;

            return ammoService.HasAmmoForReload(config.ammoType);
        }

        public ReloadResult StartReload(
            ItemInstance inst,
            WeaponConfig config,
            WeaponAmmoState ammo)
        {
            if (!CanReload(inst, config, ammo))
                return ReloadResult.CannotReload;

            ammo.isReloading = true;

            // Тактическая или полная перезарядка
            return ammo.ammoInMagazine > 0 ?
                ReloadResult.TacticalReload :
                ReloadResult.EmptyReload;
        }

        public int PerformReloadStep(
            WeaponConfig config,
            WeaponAmmoState ammo)
        {
            int space = config.magazineSize - ammo.ammoInMagazine;
            if (space <= 0)
            {
                ammo.isReloading = false;
                return 0;
            }

            int taken = ammoService.ConsumeFromInventory(config.ammoType, 1);
            if (taken <= 0)
            {
                ammo.isReloading = false;
                return 0;
            }

            int toAdd = Mathf.Min(config.ammoPerItem, space);

            ammo.ammoInMagazine += toAdd;

            if (ammo.ammoInMagazine >= config.magazineSize)
                ammo.isReloading = false;

            return toAdd;
        }


        public void FinishReload()
        {
            // UnityController сам вызывает
        }
    }

    public enum ReloadResult
    {
        CannotReload,
        TacticalReload,
        EmptyReload
    }
}
