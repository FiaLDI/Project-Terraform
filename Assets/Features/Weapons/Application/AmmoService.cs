using Features.Inventory.Application;
using Features.Items.Data;
using Features.Weapons.Domain;

namespace Features.Weapons.Application
{
    public class AmmoService
    {
        private readonly IInventoryService inventory;

        public AmmoService(IInventoryService inventory)
        {
            this.inventory = inventory;
        }

        // =====================================================
        // INVENTORY AMMO
        // =====================================================

        public int GetAmmoInInventory(Item ammoType)
        {
            return inventory.GetItemCount(ammoType);
        }

        public bool HasAmmoForReload(Item ammoType)
        {
            return inventory.GetItemCount(ammoType) > 0;
        }

        // =====================================================
        // MAGAZINE
        // =====================================================

        public int AddToMagazine(
            WeaponAmmoState ammo,
            int amount,
            int maxMagSize)
        {
            int space = maxMagSize - ammo.ammoInMagazine;
            int added = System.Math.Min(space, amount);

            ammo.ammoInMagazine += added;
            return added;
        }

        // =====================================================
        // CONSUME FROM INVENTORY
        // =====================================================

        public int ConsumeFromInventory(Item ammoType, int need)
        {
            int taken = 0;

            while (taken < need)
            {
                bool ok = inventory.TryRemove(ammoType, 1);
                if (!ok)
                    break;

                taken++;
            }

            return taken;
        }
    }
}
