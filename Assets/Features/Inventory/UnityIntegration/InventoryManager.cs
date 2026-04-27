using System;
using System.Collections.Generic;
using Features.Equipment.UnityIntegration;
using Features.Inventory.Domain;
using Features.Items.Data;
using Features.Items.Domain;
using Features.Quests.Application;
using Features.Quests.Domain;
using FishNet.Object;
using UnityEngine;

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
        private NetworkObject networkObject;

        public bool IsReady { get; private set; }

        // ======================================================
        // INIT
        // ======================================================

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
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
            NotifyInventoryChanged();
        }

        // ======================================================
        // LOAD
        // ======================================================

        public void LoadFromSave(InventorySaveData data)
        {
            if (data == null)
                data = new InventorySaveData();

            if (data.bag == null)
                data.bag = new List<ItemSaveData>();

            Model.main.Clear();

            for (int i = 0; i < bagSize; i++)
                Model.main.Add(new InventorySlot());

            for (int i = 0; i < data.bag.Count && i < bagSize; i++)
            {
                var item = data.bag[i];
                if (item == null)
                    continue;

                var def = ItemRegistrySO.Instance?.Get(item.itemId);
                if (def != null)
                    Model.main[i].item = new ItemInstance(def, item.quantity, item.level);
            }

            Model.activeSlot0.item = FromSave(data.activeSlot0);
            Model.activeSlot1.item = FromSave(data.activeSlot1);
            Model.activeSlot2.item = FromSave(data.activeSlot2);
            Model.SetActiveSlotIndex(data.activeSlotIndex);

            NotifyInventoryChanged();
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
        // BUILD SAVE
        // ======================================================

        public InventorySaveData BuildSaveData()
        {
            var data = new InventorySaveData();

            for (int i = 0; i < Model.main.Count; i++)
                data.bag.Add(ToSave(Model.main[i].item));

            data.activeSlot0 = ToSave(Model.activeSlot0.item);
            data.activeSlot1 = ToSave(Model.activeSlot1.item);
            data.activeSlot2 = ToSave(Model.activeSlot2.item);
            data.activeSlotIndex = Model.ActiveSlotIndex;

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

        public bool ConsumeActiveItem(int amount = 1)
        {
            if (Service == null)
                return false;

            return Service.ConsumeActiveItem(amount, gameObject);
        }

        public int GetItemCount(Item def)
        {
            return Service.GetItemCount(def);
        }

        public bool SetActiveSlotIndex(int index)
        {
            if (!Model.SetActiveSlotIndex(index))
                return false;

            NotifyInventoryChanged();
            return true;
        }

        public void MarkDirty()
        {
            NotifyInventoryChanged();
        }

        // ======================================================
        // NET APPLY
        // ======================================================

        public void ApplyNetState(
            IReadOnlyList<InventorySlotNet> bagNet,
            InventorySlotNet active0,
            InventorySlotNet active1,
            InventorySlotNet active2,
            int activeSlotIndex)
        {
            ApplySection(Model.main, bagNet);
            ApplySlot(Model.activeSlot0, active0);
            ApplySlot(Model.activeSlot1, active1);
            ApplySlot(Model.activeSlot2, active2);
            Model.SetActiveSlotIndex(activeSlotIndex);

            NotifyInventoryChanged();
        }

        public void ApplyActiveSlotsNetState(
            InventorySlotNet active0,
            InventorySlotNet active1,
            InventorySlotNet active2,
            int activeSlotIndex)
        {
            Model.activeSlot0.item = FromNet(active0);
            Model.activeSlot1.item = FromNet(active1);
            Model.activeSlot2.item = FromNet(active2);
            Model.SetActiveSlotIndex(activeSlotIndex);

            NotifyInventoryChanged();
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

        private void NotifyInventoryChanged()
        {
            TryPersistInventory();
            OnInventoryChanged?.Invoke();
        }

        private void TryPersistInventory()
        {
            var progress = PlayerProgressService.Instance;
            if (progress == null || progress.Data == null)
                return;

            var activeCharacter = progress.GetActiveCharacter();
            if (activeCharacter == null)
                return;

            // Save only local-owner inventory in multiplayer.
            if (networkObject != null && networkObject.IsSpawned && !networkObject.IsOwner)
                return;

            activeCharacter.characterInventoryData = BuildSaveData();
            progress.Save();
        }
    }
}

