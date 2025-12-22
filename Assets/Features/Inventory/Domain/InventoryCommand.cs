namespace Features.Inventory.Domain
{
    public enum InventoryCommand
    {
        None = 0,

        // selection
        SelectHotbar,
        PickupWorldItem,

        // move / swap
        MoveItem,

        // drop
        DropFromSlot,

        // equip
        EquipRightHand,
        EquipLeftHand,
        UnequipRightHand,
        UnequipLeftHand,

        // crafting / upgrade
        CraftRecipe,
        UpgradeItem
    }
}
