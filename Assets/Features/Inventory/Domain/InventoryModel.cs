using System.Collections.Generic;
using Features.Items.Domain;

namespace Features.Inventory.Domain
{
    public class InventoryModel
    {
        public readonly List<InventorySlot> main = new();
        public readonly List<InventorySlot> hotbar = new();

        public readonly InventorySlot leftHand  = new();
        public readonly InventorySlot rightHand = new();

        public int selectedHotbarIndex = 0;

        public IEnumerable<InventorySlot> GetAllSlots()
        {
            foreach (var s in hotbar) yield return s;
            foreach (var s in main)   yield return s;
            yield return leftHand;
            yield return rightHand;
        }

        public InventorySlot FindSlotWithInstance(ItemInstance inst)
        {
            if (inst == null || inst.IsEmpty)
                return null;

            foreach (var slot in GetAllSlots())
                if (ReferenceEquals(slot.item, inst))
                    return slot;

            return null;
        }
    }
}
