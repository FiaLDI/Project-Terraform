namespace Features.Inventory.Domain
{
    public enum InventoryCommand
    {
        None = 0,
        PickupWorldItem,
        MoveItem,
        SetActiveSlot,
        DropFromSlot,
        CraftRecipe,
        UpgradeItem,
        GiveReward
    }
}
