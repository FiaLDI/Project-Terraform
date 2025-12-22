using System;
using System.Collections.Generic;
using Features.Equipment.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.Data;
using Features.Items.Domain;
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
            // BAG
            for (int i = 0; i < Model.main.Count && i < bagNet.Count; i++)
                Model.main[i].item = FromNet(bagNet[i]);

            // HOTBAR
            for (int i = 0; i < Model.hotbar.Count && i < hotbarNet.Count; i++)
                Model.hotbar[i].item = FromNet(hotbarNet[i]);

            Model.leftHand.item = FromNet(left);
            Model.rightHand.item = FromNet(right);

            Model.selectedHotbarIndex = selectedIndex;
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
