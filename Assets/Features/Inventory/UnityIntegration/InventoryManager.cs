using System;
using System.Collections.Generic;
using Features.Equipment.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.Data;
using Features.Items.Domain;
using FishNet.Object;
using UnityEngine;

namespace Features.Inventory.UnityIntegration
{
    /// <summary>
    /// Player-scoped Inventory.
    /// Владелец модели и сервиса.
    /// НЕ содержит сетевой логики.
    /// </summary>
    public sealed class InventoryManager : MonoBehaviour, IInventoryContext
    {
        public InventoryModel Model { get; private set; }
        public InventoryService Service { get; private set; }

        public event Action OnInventoryChanged;
        public event Action<ItemInstance> OnItemAddedInstance;
        public event Action OnReady;

        [Header("Config")]
        [SerializeField] private int bagSize = 12;
        [SerializeField] private int hotbarSize = 2;

        private EquipmentManager equipment;

        public bool IsReady { get; private set; }

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            var nob = GetComponent<NetworkObject>();

            if (nob != null && !nob.IsServerInitialized)
                return;

            CreateModel();
            CreateService();
            InitEquipment();

            IsReady = true;
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
                equipment.Init(this);
            else
                Debug.LogWarning("[InventoryManager] EquipmentManager not found");
        }

        // ======================================================
        // EVENTS
        // ======================================================

        private void HandleInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        // ======================================================
        // PUBLIC API (LOCAL ONLY)
        // ======================================================

        public void AddItem(Item definition, int amount = 1)
        {
            if (definition == null || amount <= 0)
                return;

            var inst = new ItemInstance(definition, amount);

            if (!Service.AddItem(inst))
            {
                Debug.LogWarning(
                    $"[InventoryManager] Failed to add item {definition.id}");
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

        // ======================================================
        // NETWORK STATE APPLY
        // ======================================================

        public void ApplyNetState(
            IReadOnlyList<InventorySlotNet> bagNet,
            IReadOnlyList<InventorySlotNet> hotbarNet,
            InventorySlotNet left,
            InventorySlotNet right,
            int selectedIndex)
        {
            ApplySection(Model.main, bagNet);
            ApplySection(Model.hotbar, hotbarNet);

            ApplySlot(Model.leftHand, left);
            ApplySlot(Model.rightHand, right);

            Model.selectedHotbarIndex = selectedIndex;

            Debug.Log("[InventoryManager] ApplyNetState -> OnInventoryChanged");
            OnInventoryChanged?.Invoke();
        }

        private void ApplySection(
            IList<InventorySlot> slots,
            IReadOnlyList<InventorySlotNet> net)
        {
            int count = Mathf.Min(slots.Count, net.Count);
            for (int i = 0; i < count; i++)
                ApplySlot(slots[i], net[i]);
        }

        private void ApplySlot(InventorySlot slot, InventorySlotNet net)
        {
            if (string.IsNullOrEmpty(net.itemId) || net.quantity <= 0)
            {
                slot.item = ItemInstance.Empty;
                return;
            }

            if (!slot.item.IsEmpty &&
                slot.item.itemDefinition.id == net.itemId &&
                slot.item.level == net.level)
            {
                slot.item.quantity = net.quantity;
                return;
            }

            // NEW ITEM
            var def = ItemRegistrySO.Instance?.Get(net.itemId);
            if (def == null)
            {
                Debug.LogError($"[Inventory] Item not found: {net.itemId}");
                slot.item = ItemInstance.Empty;
                return;
            }

            slot.item = new ItemInstance(def, net.quantity, net.level);
        }


        public void ApplyHandsNetState(InventorySlotNet left, InventorySlotNet right)
        {
            Model.leftHand.item  = FromNet(left);
            Model.rightHand.item = FromNet(right);
            OnInventoryChanged?.Invoke();
        }

        private ItemInstance FromNet(InventorySlotNet net)
        {
            if (string.IsNullOrEmpty(net.itemId) || net.quantity <= 0)
                return ItemInstance.Empty;

            var def = ItemRegistrySO.Instance?.Get(net.itemId);
            if (def == null)
            {
                Debug.LogError($"[InventoryManager] Item not found: {net.itemId}");
                return ItemInstance.Empty;
            }

            return new ItemInstance(def, net.quantity, net.level);
        }
    }
}
