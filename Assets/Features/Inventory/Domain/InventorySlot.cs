using Features.Items.Domain;

namespace Features.Inventory.Domain
{
    public class InventorySlot
    {
        public ItemInstance item;
        
        public bool IsEmpty => item == null;
    }
}
