using Features.Inventory.Domain;
using Features.Items.Domain;

public struct InventorySlotRef
{
    public InventorySection Section;
    public int Index;
    public ItemInstance Item;

    public InventorySlotRef(InventorySection section, int index)
    {
        Section = section;
        Index   = index;
        Item    = null;
    }

    public InventorySlotRef(InventorySection section, int index, ItemInstance item)
    {
        Section = section;
        Index   = index;
        Item    = item;
    }
}
