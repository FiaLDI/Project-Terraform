using System;
using System.Linq;
using Features.Inventory.Application;
using Features.Items.Data;
using Features.Items.Domain;

namespace Features.Inventory.Domain
{
    /// <summary>
    /// Application-слой инвентаря:
    /// - добавление / удаление предметов
    /// - перемещение
    /// - экипировка
    /// - обработка двуручности
    /// 
    /// НЕ содержит Unity API.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly InventoryModel model;

        public event Action OnChanged;

        public event Action<ItemInstance> OnItemAdded;

        public InventoryService(InventoryModel model)
        {
            this.model = model;
        }

        // ===============================================================
        // ADD ITEM
        // ===============================================================

        public bool AddItem(ItemInstance inst)
        {
            // 1. Попытка стакнуть в хотбар
            if (inst.IsStackable)
            {
                var stack = model.hotbar
                    .Concat(model.main)
                    .FirstOrDefault(s =>
                        s.item != null &&
                        s.item.itemDefinition == inst.itemDefinition &&
                        s.item.level == inst.level &&
                        s.item.quantity < inst.MaxStack);

                if (stack != null)
                {
                    int canAdd = inst.MaxStack - stack.item.quantity;
                    int add = Math.Min(canAdd, inst.quantity);

                    stack.item.quantity += add;
                    inst.quantity -= add;

                    if (inst.quantity <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }

            // 2. Сначала пустые ячейки хотбара
            foreach (var slot in model.hotbar)
            {
                if (slot.item == null)
                {
                    slot.item = inst;
                    OnChanged?.Invoke();
                    return true;
                }
            }

            // 3. Потом сумка
            foreach (var slot in model.main)
            {
                if (slot.item == null)
                {
                    slot.item = inst;
                    OnChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }

        // ===============================================================
        // REMOVE
        // ===============================================================

        public bool TryRemove(Item def, int count)
        {
            int left = count;

            foreach (var slot in model.hotbar)
            {
                if (slot.item != null && slot.item.itemDefinition == def)
                {
                    int take = Math.Min(left, slot.item.quantity);
                    slot.item.quantity -= take;
                    left -= take;

                    if (slot.item.quantity <= 0)
                        slot.item = null;

                    if (left <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }

            foreach (var slot in model.main)
            {
                if (slot.item != null && slot.item.itemDefinition == def)
                {
                    int take = Math.Min(left, slot.item.quantity);
                    slot.item.quantity -= take;
                    left -= take;

                    if (slot.item.quantity <= 0)
                        slot.item = null;

                    if (left <= 0)
                    {
                        OnChanged?.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }

        // ===============================================================
        // COUNT
        // ===============================================================

        public int GetItemCount(Item def)
        {
            return model.hotbar
                .Concat(model.main)
                .Where(s => s.item != null && s.item.itemDefinition == def)
                .Sum(s => s.item.quantity);
        }

        // ===============================================================
        // MOVE ITEM
        // ===============================================================

        public bool MoveItem(int fromIndex, InventorySection fromSection,
                             int toIndex, InventorySection toSection)
        {
            var from = GetSlot(fromSection, fromIndex);
            var to = GetSlot(toSection, toIndex);

            if (from == null || to == null)
                return false;

            var tmp = to.item;
            to.item = from.item;
            from.item = tmp;

            OnChanged?.Invoke();
            return true;
        }

        // ===============================================================
        // EQUIP
        // ===============================================================

        public void EquipRightHand(int slot, InventorySection section)
        {
            var s = GetSlot(section, slot);
            model.rightHand.item = s.item;

            // двуручность
            HandleTwoHandedIfNeeded();

            OnChanged?.Invoke();
        }

        public void EquipLeftHand(int slot, InventorySection section)
        {
            var s = GetSlot(section, slot);
            model.leftHand.item = s.item;

            OnChanged?.Invoke();
        }

        public void UnequipRightHand()
        {
            model.rightHand.item = null;
            OnChanged?.Invoke();
        }

        public void UnequipLeftHand()
        {
            model.leftHand.item = null;
            OnChanged?.Invoke();
        }

        // ===============================================================
        // TWO-HANDED ITEM LOGIC
        // ===============================================================

        public void HandleTwoHandedIfNeeded()
        {
            var item = model.rightHand.item;
            if (item != null && item.itemDefinition.isTwoHanded)
            {
                model.leftHand.item = null;
            }
        }

        // ===============================================================
        // HELPERS
        // ===============================================================

        private InventorySlot GetSlot(InventorySection section, int index)
        {
            return section switch
            {
                InventorySection.Hotbar => model.hotbar[index],
                InventorySection.Bag => model.main[index],
                InventorySection.LeftHand => model.leftHand,
                InventorySection.RightHand => model.rightHand,
                _ => null
            };
        }

        public ItemInstance GetFirst(Item def)
        {
            if (def == null)
                return null;

            // 1) hotbar
            foreach (var slot in model.hotbar)
            {
                if (slot.item != null && slot.item.itemDefinition == def)
                    return slot.item;
            }

            // 2) bag
            foreach (var slot in model.main)
            {
                if (slot.item != null && slot.item.itemDefinition == def)
                    return slot.item;
            }

            return null;
        }

        public bool HasIngredients(RecipeIngredient[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0)
                return true;

            foreach (var ing in ingredients)
            {
                if (ing.item == null)
                    continue;

                int have = GetItemCount(ing.item);
                if (have < ing.amount)
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
                if (ing.item == null)
                    continue;

                TryRemove(ing.item, ing.amount);
            }

            return true;
        }

        public (InventorySection section, int index)? FindSlot(ItemInstance inst)
        {
            // hotbar
            for (int i = 0; i < model.hotbar.Count; i++)
            {
                if (model.hotbar[i].item == inst)
                    return (InventorySection.Hotbar, i);
            }

            // bag
            for (int i = 0; i < model.main.Count; i++)
            {
                if (model.main[i].item == inst)
                    return (InventorySection.Bag, i);
            }

            return null;
        }

        public void SelectHotbarIndex(int index)
        {
            if (index < 0 || index >= model.hotbar.Count)
                return;

            model.selectedHotbarIndex = index;
            OnChanged?.Invoke();
        }

    }
}
