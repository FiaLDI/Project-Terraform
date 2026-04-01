namespace Features.Inventory.Domain
{
    public enum InventoryCommand
    {
        None = 0,
        PickupWorldItem,
        MoveItem,
        DropFromSlot,
        EquipRightHand,
        EquipLeftHand,
        UnequipRightHand,
        UnequipLeftHand,
        CraftRecipe,
        UpgradeItem,
        GiveReward
    }
}
