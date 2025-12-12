using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.Application;
using Features.Items.Domain;
using Features.Items.Data;
using System;
using Features.Inventory;

namespace Features.Inventory.UnityIntegration
{
    /// <summary>
    /// Player-scoped Inventory.
    /// Никаких singleton'ов.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IInventoryContext
    {
        public InventoryModel Model { get; private set; }
        public InventoryService Service { get; private set; }

        public event Action OnInventoryChanged;
        public event Action<ItemInstance> OnItemAddedInstance;

        [SerializeField] private int bagSize = 12;
        [SerializeField] private int hotbarSize = 2;

        private void Awake()
        {
            CreateModel();
            CreateService();
        }

        private void CreateModel()
        {
            Model = new InventoryModel();

            for (int i = 0; i < bagSize; i++)
                Model.main.Add(new InventorySlot());

            for (int i = 0; i < hotbarSize; i++)
                Model.hotbar.Add(new InventorySlot());
        }

        private void CreateService()
        {
            Service = new InventoryService(Model);
            Service.OnChanged += () => OnInventoryChanged?.Invoke();
        }

        // ======================================================
        // PUBLIC API — NO GAME LOGIC
        // ======================================================

        public void AddItem(Item definition, int amount = 1)
        {
            if (definition == null || amount <= 0)
                return;

            var inst = new ItemInstance(definition, amount);
            Service.AddItem(inst);

            OnItemAddedInstance?.Invoke(inst);
        }

        public bool RemoveItem(Item definition, int amount = 1)
        {
            return Service.TryRemove(definition, amount);
        }

        public int GetItemCount(Item definition)
        {
            return Service.GetItemCount(definition);
        }

        public void SelectHotbarIndex(int index)
        {
            Model.selectedHotbarIndex =
                Mathf.Clamp(index, 0, Model.hotbar.Count - 1);

            OnInventoryChanged?.Invoke();
        }

        // ======================================================
        // EQUIPMENT SHORTCUTS (UI ONLY)
        // ======================================================

        public void EquipRightFromUI(int slotIndex, InventorySection section)
        {
            Service.EquipRightHand(slotIndex, section);
        }

        public void EquipLeftFromUI(int slotIndex, InventorySection section)
        {
            Service.EquipLeftHand(slotIndex, section);
        }

        public void UnequipRight()
        {
            Service.UnequipRightHand();
        }

        public void UnequipLeft()
        {
            Service.UnequipLeftHand();
        }
    }
}
