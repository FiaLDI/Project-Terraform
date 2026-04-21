using System.Collections.Generic;
using Features.Items.Domain;

namespace Features.Inventory.Domain
{
    public class InventoryModel
    {
        public const int ActiveSlotCount = 3;

        public readonly List<InventorySlot> main = new();

        public readonly InventorySlot activeSlot0 = new();
        public readonly InventorySlot activeSlot1 = new();
        public readonly InventorySlot activeSlot2 = new();

        public int ActiveSlotIndex { get; private set; }

        public IEnumerable<InventorySlot> GetAllSlots()
        {
            foreach (var s in main)   yield return s;
            yield return activeSlot0;
            yield return activeSlot1;
            yield return activeSlot2;
        }

        public IEnumerable<InventorySlot> GetActiveSlots()
        {
            yield return activeSlot0;
            yield return activeSlot1;
            yield return activeSlot2;
        }

        public InventorySlot GetActiveSlot(int index)
        {
            return ClampActiveSlotIndex(index) switch
            {
                0 => activeSlot0,
                1 => activeSlot1,
                _ => activeSlot2
            };
        }

        public bool SetActiveSlotIndex(int index)
        {
            int clamped = ClampActiveSlotIndex(index);

            if (ActiveSlotIndex == clamped)
                return false;

            ActiveSlotIndex = clamped;
            return true;
        }

        public static int ClampActiveSlotIndex(int index)
        {
            if (index < 0)
                return 0;

            if (index >= ActiveSlotCount)
                return ActiveSlotCount - 1;

            return index;
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
