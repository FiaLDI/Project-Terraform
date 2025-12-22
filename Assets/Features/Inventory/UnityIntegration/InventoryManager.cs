using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.Application;
using Features.Items.Domain;
using Features.Items.Data;
using System;
using Features.Inventory;
using Features.Equipment.UnityIntegration;
using System.Collections.Generic;

namespace Features.Inventory.UnityIntegration
{
    /// <summary>
    /// Player-scoped Inventory.
    /// Владелец модели, сервиса и экипировки.
    /// Никаких singleton'ов.
    /// </summary>
    public class InventoryManager : MonoBehaviour, IInventoryContext
    {
        public InventoryModel Model { get; private set; }
        public InventoryService Service { get; private set; }

        public event Action OnInventoryChanged;
        public event Action<ItemInstance> OnItemAddedInstance;
        public event Action OnReady;

        [SerializeField] private ItemRegistry itemRegistry;


        [Header("Config")]
        [SerializeField] private int bagSize = 12;
        [SerializeField] private int hotbarSize = 2; // руки

        private EquipmentManager equipment;

        public bool IsReady { get; private set; }

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            CreateModel();
            CreateService();
            InitEquipment();

            IsReady = true;
            Debug.Log("XXX[InventoryManager] OnReady invoked", this);
            OnReady?.Invoke();
        }

        private void OnDestroy()
        {
            if (Service != null)
                Service.OnChanged -= HandleInventoryChanged;
        }

        // ======================================================
        // INIT
        // ======================================================

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
            Service.OnChanged += HandleInventoryChanged;
        }

        private void InitEquipment()
        {
            equipment = GetComponent<EquipmentManager>();
            if (equipment != null)
            {
                equipment.Init(this);
            }
            else
            {
                Debug.LogWarning(
                    "[InventoryManager] EquipmentManager not found on Player"
                );
            }
        }

        // ======================================================
        // EVENTS
        // ======================================================

        private void HandleInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        // ======================================================
        // PUBLIC API — NO GAME LOGIC
        // ======================================================

        public void AddItem(Item definition, int amount = 1)
        {
            if (definition == null || amount <= 0)
                return;

            var inst = new ItemInstance(definition, amount);

            bool added = Service.AddItem(inst);
            if (!added)
            {
                Debug.LogWarning(
                    $"[InventoryManager] Failed to add item {definition.name}"
                );
                return;
            }

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

        public void ApplyNetState(
            IReadOnlyList<InventorySlotNet> bagNet,
            IReadOnlyList<InventorySlotNet> hotbarNet,
            InventorySlotNet left,
            InventorySlotNet right,
            int selectedIndex)
        {
            Model.main.Clear();
            Model.hotbar.Clear();



            foreach (var s in bagNet) {
                var slot = new InventorySlot();
                slot.item = FromNet(s);
                Model.main.Add(slot);
            }

            foreach (var s in hotbarNet)
            {
                var slot = new InventorySlot();
                slot.item = FromNet(s);
                Model.hotbar.Add(slot);
            }

            Model.leftHand.item = FromNet(left);
            Model.rightHand.item = FromNet(right);
            Model.selectedHotbarIndex = selectedIndex;

            Service.NotifyChanged();
        }

        private ItemInstance FromNet(InventorySlotNet net)
        {
            if (string.IsNullOrEmpty(net.itemId))
                return null;

            var def = itemRegistry.Get(net.itemId);
            if (def == null)
            {
                Debug.LogError($"Item not found: {net.itemId}");
                return null;
            }

            return new ItemInstance(def, net.quantity, net.level);
        }


    }
}
