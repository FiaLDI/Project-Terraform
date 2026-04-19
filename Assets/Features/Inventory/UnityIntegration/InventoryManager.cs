using System;
using System.Collections.Generic;
using Features.Equipment.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.Data;
using Features.Items.Domain;
using Features.Quests.Application;
using Features.Quests.Domain;
using UnityEngine;
using FishNet.Object;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryManager : MonoBehaviour, IInventoryContext
    {
        public InventoryModel Model { get; private set; }
        public InventoryService Service { get; private set; }

        public event Action OnInventoryChanged;
        public event Action<ItemInstance> OnItemAddedInstance;
        public event Action OnReady;

        [SerializeField] private int bagSize = 12;

        private EquipmentManager equipment;

        private bool isLoading;
        private bool receivedFirstSync;

        public bool IsReady { get; private set; }

        // ======================================================
        // INIT
        // ======================================================

        private void Awake()
        {
            CreateModel();
            CreateService();
            InitEquipment();

            Service.OnItemAdded += HandleItemAdded;

            IsReady = true;
            OnReady?.Invoke();
        }

        private void OnDestroy()
        {
            if (Service != null)
            {
                Service.OnChanged -= HandleInventoryChanged;
                Service.OnItemAdded -= HandleItemAdded;
            }
        }

        private void CreateModel()
        {
            Model = new InventoryModel();

            for (int i = 0; i < bagSize; i++)
                Model.main.Add(new InventorySlot());
        }

        private void CreateService()
        {
            Service = new InventoryService(Model);
            Service.OnChanged += HandleInventoryChanged;
        }

        private void InitEquipment()
        {
            equipment = GetComponent<EquipmentManager>();
            equipment?.Init(this);
        }

        private void HandleInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        // ======================================================
        // LOAD (FIXED)
        // ======================================================

        public void LoadFromSave(InventorySaveData data)
        {
            // 🔥 КРИТИЧЕСКИЙ ФИКС
            if (data == null)
                data = new InventorySaveData();

            if (data.bag == null)
                data.bag = new List<ItemSaveData>();

            isLoading = true;

            Model.main.Clear();

            for (int i = 0; i < bagSize; i++)
                Model.main.Add(new InventorySlot());

            for (int i = 0; i < data.bag.Count && i < bagSize; i++)
            {
                var item = data.bag[i];
                if (item == null) continue;

                var def = ItemRegistrySO.Instance?.Get(item.itemId);
                if (def != null)
                {
                    Model.main[i].item =
                        new ItemInstance(def, item.quantity, item.level);
                }
            }

            Model.leftHand.item  = FromSave(data.leftHand);
            Model.rightHand.item = FromSave(data.rightHand);

            isLoading = false;

            OnInventoryChanged?.Invoke();
        }

        private ItemInstance FromSave(ItemSaveData data)
        {
            if (data == null)
                return ItemInstance.Empty;

            var def = ItemRegistrySO.Instance?.Get(data.itemId);
            if (def == null)
                return ItemInstance.Empty;

            return new ItemInstance(def, data.quantity, data.level);
        }

        // ======================================================
        // BUILD SAVE (использует сервер)
        // ======================================================

        public InventorySaveData BuildSaveData()
        {
            var data = new InventorySaveData();

            for (int i = 0; i < Model.main.Count; i++)
                data.bag.Add(ToSave(Model.main[i].item));

            data.leftHand  = ToSave(Model.leftHand.item);
            data.rightHand = ToSave(Model.rightHand.item);

            return data;
        }

        private ItemSaveData ToSave(ItemInstance inst)
        {
            if (inst == null || inst.IsEmpty)
                return null;

            return new ItemSaveData
            {
                itemId = inst.itemDefinition.id,
                quantity = inst.quantity,
                level = inst.level
            };
        }

        // ======================================================
        // API
        // ======================================================

        public void AddItem(Item def, int amount = 1)
        {
            if (def == null || amount <= 0)
                return;

            var inst = new ItemInstance(def, amount);

            if (!Service.AddItem(inst))
                return;

            OnItemAddedInstance?.Invoke(inst);
        }

        public bool RemoveItem(Item def, int amount = 1)
        {
            return Service.TryRemove(def, amount);
        }

        public int GetItemCount(Item def)
        {
            return Service.GetItemCount(def);
        }

        // ======================================================
        // NET APPLY
        // ======================================================

        public void ApplyNetState(
            IReadOnlyList<InventorySlotNet> bagNet,
            InventorySlotNet left,
            InventorySlotNet right)
        {
            isLoading = true;

            ApplySection(Model.main, bagNet);
            ApplySlot(Model.leftHand, left);
            ApplySlot(Model.rightHand, right);

            isLoading = false;
            receivedFirstSync = true;

            OnInventoryChanged?.Invoke();
        }

        public void ApplyHandsNetState(InventorySlotNet left, InventorySlotNet right)
        {
            Model.leftHand.item = FromNet(left);
            Model.rightHand.item = FromNet(right);

            OnInventoryChanged?.Invoke();
        }

        private void ApplySection(IList<InventorySlot> slots, IReadOnlyList<InventorySlotNet> net)
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

            var def = ItemRegistrySO.Instance?.Get(net.itemId);
            if (def == null)
            {
                slot.item = ItemInstance.Empty;
                return;
            }

            slot.item = new ItemInstance(def, net.quantity, net.level);
        }

        private ItemInstance FromNet(InventorySlotNet net)
        {
            if (string.IsNullOrEmpty(net.itemId) || net.quantity <= 0)
                return ItemInstance.Empty;

            var def = ItemRegistrySO.Instance?.Get(net.itemId);
            if (def == null)
                return ItemInstance.Empty;

            return new ItemInstance(def, net.quantity, net.level);
        }

        // ======================================================
        // QUEST EVENTS
        // ======================================================

        private void HandleItemAdded(ItemInstance inst, int addedAmount)
        {
            if (inst == null || inst.IsEmpty || addedAmount <= 0)
                return;

            QuestEventBus.Publish(
                new ItemAddedEvent(
                    gameObject,
                    inst.itemDefinition.id,
                    addedAmount
                )
            );
        }
    }
}
