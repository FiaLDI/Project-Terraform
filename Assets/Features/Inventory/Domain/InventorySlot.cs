using System;
using Features.Items.Domain;

namespace Features.Inventory.Domain
{
    public class InventorySlot
    {
        public ItemInstance item = ItemInstance.Empty;

        public bool IsEmpty => item.IsEmpty;

        internal void Clear()
        {
            this.item = ItemInstance.Empty;
        }
    }
}
