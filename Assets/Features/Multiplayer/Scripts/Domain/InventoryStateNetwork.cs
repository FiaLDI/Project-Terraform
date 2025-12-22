using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Inventory.UnityIntegration;
using Features.Inventory.Domain;

[RequireComponent(typeof(InventoryManager))]
public sealed class InventoryStateNetwork : NetworkBehaviour
{
    // ================= SYNC DATA =================

    public readonly SyncList<InventorySlotNet> bag = new();
    public readonly SyncList<InventorySlotNet> hotbar = new();

    private readonly SyncVar<InventorySlotNet> leftHand = new();
    private readonly SyncVar<InventorySlotNet> rightHand = new();
    private readonly SyncVar<int> selectedHotbarIndex = new();

    private InventoryManager inventory;

    // ================= LIFECYCLE =================

    public override void OnStartServer()
    {
        base.OnStartServer();

        inventory = GetComponent<InventoryManager>();
        inventory.Service.OnChanged += PublishState;

        PublishState();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (inventory != null)
            inventory.Service.OnChanged -= PublishState;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        inventory = GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError(
                "[InventoryStateNetwork] InventoryManager missing on client",
                this);
            return;
        }

        bag.OnChange += OnBagChanged;
        hotbar.OnChange += OnHotbarChanged;

        leftHand.OnChange += OnSlotChanged;
        rightHand.OnChange += OnSlotChanged;
        selectedHotbarIndex.OnChange += OnIndexChanged;

        ApplyToClient();
    }


    public override void OnStopClient()
    {
        base.OnStopClient();

        bag.OnChange -= OnBagChanged;
        hotbar.OnChange -= OnHotbarChanged;

        leftHand.OnChange -= OnSlotChanged;
        rightHand.OnChange -= OnSlotChanged;
        selectedHotbarIndex.OnChange -= OnIndexChanged;
    }

    // ================= SERVER =================

    private void PublishState()
    {
        if (!IsServerInitialized)
            return;

        bag.Clear();
        foreach (var s in inventory.Model.main)
            bag.Add(ToNet(s));

        hotbar.Clear();
        foreach (var s in inventory.Model.hotbar)
            hotbar.Add(ToNet(s));

        leftHand.Value = ToNet(inventory.Model.leftHand);
        rightHand.Value = ToNet(inventory.Model.rightHand);
        selectedHotbarIndex.Value = inventory.Model.selectedHotbarIndex;
    }

    // ================= CLIENT =================

    private void ApplyToClient()
    {
        if (IsServerInitialized || inventory == null)
            return;

        inventory.ApplyNetState(
            bag,
            hotbar,
            leftHand.Value,
            rightHand.Value,
            selectedHotbarIndex.Value
        );
    }


    private void OnBagChanged(
        SyncListOperation op,
        int index,
        InventorySlotNet oldItem,
        InventorySlotNet newItem,
        bool asServer)
    {
        ApplyToClient();
    }

    private void OnHotbarChanged(
        SyncListOperation op,
        int index,
        InventorySlotNet oldItem,
        InventorySlotNet newItem,
        bool asServer)
    {
        ApplyToClient();
    }

    private void OnSlotChanged(
        InventorySlotNet oldVal,
        InventorySlotNet newVal,
        bool asServer)
    {
        ApplyToClient();
    }

    private void OnIndexChanged(
        int oldVal,
        int newVal,
        bool asServer)
    {
        ApplyToClient();
    }

    // ================= CONVERSION =================

    private InventorySlotNet ToNet(InventorySlot slot)
    {
        if (slot.item == null)
            return default;

        return new InventorySlotNet
        {
            itemId = slot.item.itemDefinition.id,
            quantity = slot.item.quantity,
            level = slot.item.level
        };
    }

    // ================= RPC API (CLIENT → SERVER) =================

    [ServerRpc]
    public void RequestMoveItem(
        int fromIdx,
        InventorySection from,
        int toIdx,
        InventorySection to)
    {
        inventory.Service.MoveItem(fromIdx, from, toIdx, to);
    }

    [ServerRpc]
    public void RequestEquipRight(int index, InventorySection section)
    {
        inventory.Service.EquipRightHand(index, section);
    }

    [ServerRpc]
    public void RequestEquipLeft(int index, InventorySection section)
    {
        inventory.Service.EquipLeftHand(index, section);
    }
}
