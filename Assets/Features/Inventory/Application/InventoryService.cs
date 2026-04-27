using System;
using System.Linq;
using Features.Inventory.Application;
using Features.Items.Data;
using Features.Items.Domain;
using Features.Quests.Application;
using Features.Quests.Domain;

namespace Features.Inventory.Domain
{
    /// <summary>
    /// Application layer of inventory.
    /// Does not use Unity API except debug/event integration.
    /// Works only with ItemInstance.Empty sentinel for empty slots.
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly InventoryModel model;

        public event Action OnChanged;
        public event Action<ItemInstance, int> OnItemAdded;

        public InventoryService(InventoryModel model)
        {
            this.model = model;
        }

        // =====================================================
        // ADD
        // =====================================================

        public bool AddItem(ItemInstance inst)
        {
            if (inst == null || inst.IsEmpty || inst.quantity <= 0)
                return false;

            int remaining = inst.quantity;

            if (inst.IsStackable)
            {
                foreach (var slot in model.main)
                {
                    var item = slot.item;
                    if (item.IsEmpty)
                        continue;

                    if (item.itemDefinition != inst.itemDefinition ||
                        item.level != inst.level ||
                        item.quantity >= item.MaxStack)
                        continue;

                    int add = Math.Min(item.MaxStack - item.quantity, remaining);
                    item.quantity += add;
                    remaining -= add;

                    OnItemAdded?.Invoke(item, add);

                    if (remaining <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }

            foreach (var slot in model.main)
            {
                if (!slot.item.IsEmpty)
                    continue;

                slot.item = new ItemInstance(
                    inst.itemDefinition,
                    remaining,
                    inst.level
                );

                OnItemAdded?.Invoke(slot.item, remaining);
                OnChanged?.Invoke();
                return true;
            }

            return false;
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public bool TryRemove(Item def, int count, object source = null)
        {
            int left = count;

            foreach (var slot in model.main)
            {
                var item = slot.item;
                if (item.IsEmpty || item.itemDefinition != def)
                    continue;

                int take = Math.Min(left, item.quantity);
                item.quantity -= take;
                left -= take;

                if (item.quantity <= 0)
                    slot.item = ItemInstance.Empty;

                if (left <= 0)
                {
                    OnChanged?.Invoke();

                    if (source is UnityEngine.GameObject go)
                    {
                        QuestEventBus.Publish(
                            new ItemRemovedEvent(go, def.id, count)
                        );
                    }

                    return true;
                }
            }

            return false;
        }

        // =====================================================
        // COUNT
        // =====================================================

        public int GetItemCount(Item def)
        {
            return model.main
                .Where(s => !s.item.IsEmpty && s.item.itemDefinition == def)
                .Sum(s => s.item.quantity);
        }

        // =====================================================
        // MOVE
        // =====================================================

        public bool MoveItem(
            int fromIndex,
            InventorySection fromSection,
            int toIndex,
            InventorySection toSection)
        {
            var from = GetSlot(fromSection, fromIndex);
            var to = GetSlot(toSection, toIndex);

            if (from == null || to == null || from.item.IsEmpty)
                return false;

            Swap(from, to);

            OnChanged?.Invoke();
            return true;
        }

        // =====================================================
        // DROP / EXTRACT
        // =====================================================

        public ItemInstance ExtractFromSlot(
            InventorySection section,
            int index,
            int amount)
        {
            return section switch
            {
                InventorySection.Bag =>
                    ExtractFromBag(index, amount),

                InventorySection.ActiveSlot0 or InventorySection.ActiveSlot1 or
                InventorySection.ActiveSlot2 =>
                    ExtractFromActiveSlot(section),

                _ => ItemInstance.Empty
            };
        }

        private ItemInstance ExtractFromBag(int index, int amount)
        {
            if (index < 0 || index >= model.main.Count)
                return ItemInstance.Empty;

            var slot = model.main[index];
            return ExtractFromSlotInternal(slot, amount);
        }

        private ItemInstance ExtractFromSlotInternal(
            InventorySlot slot,
            int amount)
        {
            var inst = slot.item;
            if (inst.IsEmpty)
                return ItemInstance.Empty;

            int take = Math.Min(amount, inst.quantity);
            var extracted = inst.CloneWithQuantity(take);

            inst.quantity -= take;
            if (inst.quantity <= 0)
                slot.item = ItemInstance.Empty;

            OnChanged?.Invoke();
            return extracted;
        }

        private ItemInstance ExtractFromActiveSlot(InventorySection section)
        {
            var slot = GetSlot(section, 0);
            if (slot == null || slot.item.IsEmpty)
                return ItemInstance.Empty;

            var dropped = slot.item;
            slot.item = ItemInstance.Empty;

            OnChanged?.Invoke();
            return dropped;
        }

        public ItemInstance DropFromActiveSlots()
        {
            var selectedSlot = model.GetActiveSlot(model.ActiveSlotIndex);

            if (selectedSlot != null && !selectedSlot.item.IsEmpty)
            {
                var item = selectedSlot.item;
                UnityEngine.Debug.Log(
                    $"[InventoryService] DropFromActiveSlots ACTIVE({model.ActiveSlotIndex}): def={(item.itemDefinition != null ? item.itemDefinition.name : "NULL")}, " +
                    $"id='{item.itemDefinition?.id}', qty={item.quantity}, level={item.level}"
                );

                var dropped = selectedSlot.item;
                selectedSlot.item = ItemInstance.Empty;
                OnChanged?.Invoke();
                return dropped;
            }

            for (int i = 0; i < InventoryModel.ActiveSlotCount; i++)
            {
                var fallbackSlot = model.GetActiveSlot(i);
                if (fallbackSlot == null || fallbackSlot.item.IsEmpty)
                    continue;

                var dropped = fallbackSlot.item;
                fallbackSlot.item = ItemInstance.Empty;
                OnChanged?.Invoke();
                return dropped;
            }

            return ItemInstance.Empty;
        }

        public bool ConsumeActiveItem(int amount = 1, object source = null)
        {
            if (amount <= 0)
                return false;

            var slot = model.GetActiveSlot(model.ActiveSlotIndex);
            if (slot == null || slot.item.IsEmpty)
                return false;

            var item = slot.item;
            var def = item.itemDefinition;
            int consumed = Math.Min(amount, item.quantity);

            item.quantity -= consumed;
            if (item.quantity <= 0)
                slot.item = ItemInstance.Empty;

            OnChanged?.Invoke();

            if (source is UnityEngine.GameObject go && def != null)
            {
                QuestEventBus.Publish(
                    new ItemRemovedEvent(go, def.id, consumed)
                );
            }

            return true;
        }

        // =====================================================
        // INGREDIENTS
        // =====================================================

        public bool HasIngredients(RecipeIngredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0)
                return true;

            foreach (var ing in ingredients)
            {
                if (ing.item == null)
                    continue;

                if (GetItemCount(ing.item) < ing.amount)
                    return false;
            }

            return true;
        }

        public bool ConsumeIngredients(RecipeIngredient[] ingredients)
        {
            if (!HasIngredients(ingredients))
                return false;

            foreach (var ing in ingredients)
            {
                if (ing.item != null)
                    TryRemove(ing.item, ing.amount);
            }

            return true;
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private InventorySlot GetSlot(InventorySection section, int index)
        {
            return section switch
            {
                InventorySection.Bag => model.main[index],
                InventorySection.ActiveSlot0 => model.activeSlot0,
                InventorySection.ActiveSlot1 => model.activeSlot1,
                InventorySection.ActiveSlot2 => model.activeSlot2,
                _ => null
            };
        }

        private static void Swap(InventorySlot a, InventorySlot b)
        {
            var tmp = a.item;
            a.item = b.item;
            b.item = tmp;
        }
    }
}

