using System;
using System.Linq;
using Features.Inventory.Application;
using Features.Items.Data;
using Features.Items.Domain;


namespace Features.Inventory.Domain
{
    /// <summary>
    /// Application-слой инвентаря.
    /// НЕ использует Unity API.
    /// Работает ТОЛЬКО с ItemInstance.Empty.
    /// </summary>
    public sealed class InventoryService : IInventoryService
    {
        private readonly InventoryModel model;


        public event Action OnChanged;
        public event Action<ItemInstance> OnItemAdded;


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
                    if (item.IsEmpty) continue;
                    if (item.itemDefinition != inst.itemDefinition ||
                        item.level != inst.level ||
                        item.quantity >= item.MaxStack)
                        continue;


                    int add = Math.Min(item.MaxStack - item.quantity, remaining);
                    item.quantity += add;
                    remaining -= add;


                    if (remaining <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }


            foreach (var slot in model.main)
            {
                if (!slot.item.IsEmpty) continue;


                slot.item = new ItemInstance(
                    inst.itemDefinition,
                    remaining,
                    inst.level
                );


                OnItemAdded?.Invoke(slot.item);
                OnChanged?.Invoke();
                return true;
            }


            return false;
        }



        // =====================================================
        // REMOVE
        // =====================================================


        public bool TryRemove(Item def, int count)
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
            var to   = GetSlot(toSection, toIndex);


            if (from == null || to == null || from.item.IsEmpty)
                return false;


            // forbid: two-handed → left hand
            if (toSection == InventorySection.LeftHand &&
                model.rightHand.item.itemDefinition?.isTwoHanded == true)
                return false;


            // two-handed → right clears left
            if (toSection == InventorySection.RightHand &&
                from.item.itemDefinition?.isTwoHanded == true)
                model.leftHand.item = ItemInstance.Empty;


            Swap(from, to);
            HandleTwoHandedIfNeeded();


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

                InventorySection.LeftHand or InventorySection.RightHand =>
                    DropFromHands(),

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


        public ItemInstance DropFromHands()
{
    if (!model.rightHand.item.IsEmpty)
    {
        var item = model.rightHand.item;
        UnityEngine.Debug.Log(
            $"[InventoryService] DropFromHands RIGHT: def={(item.itemDefinition != null ? item.itemDefinition.name : "NULL")}, " +
            $"id='{item.itemDefinition?.id}', qty={item.quantity}, level={item.level}"
        );

        var dropped = model.rightHand.item;
        model.rightHand.item = ItemInstance.Empty;
        OnChanged?.Invoke();
        return dropped;
    }

    if (!model.leftHand.item.IsEmpty)
    {
        var item = model.leftHand.item;
        UnityEngine.Debug.Log(
            $"[InventoryService] DropFromHands LEFT: def={(item.itemDefinition != null ? item.itemDefinition.name : "NULL")}, " +
            $"id='{item.itemDefinition?.id}', qty={item.quantity}, level={item.level}"
        );

        var dropped = model.leftHand.item;
        model.leftHand.item = ItemInstance.Empty;
        OnChanged?.Invoke();
        return dropped;
    }

    return ItemInstance.Empty;
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
                InventorySection.Bag      => model.main[index],
                InventorySection.LeftHand => model.leftHand,
                InventorySection.RightHand=> model.rightHand,
                _ => null
            };
        }


        private void HandleTwoHandedIfNeeded()
        {
            var item = model.rightHand.item;
            if (!item.IsEmpty && item.itemDefinition.isTwoHanded)
                model.leftHand.item = ItemInstance.Empty;
        }


        private static void Swap(InventorySlot a, InventorySlot b)
        {
            var tmp = a.item;
            a.item = b.item;
            b.item = tmp;
        }
    }
}