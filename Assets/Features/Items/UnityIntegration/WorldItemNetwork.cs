using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Items.UnityIntegration;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Collider))]
public sealed class WorldItemNetwork : NetworkBehaviour
{
    private readonly SyncVar<string> itemId = new();
    private readonly SyncVar<int> quantity = new();
    private readonly SyncVar<int> level = new();

    private ItemRuntimeHolder runtimeHolder;
    private bool runtimeApplied;

    // ================= PUBLIC API =================

    public bool IsPickupAvailable =>
        !string.IsNullOrEmpty(itemId.Value) &&
        quantity.Value > 0;

    public string ItemId => itemId.Value;
    public int Quantity => quantity.Value;
    public int Level => level.Value;

    // ================= SERVER INIT =================

    [Server]
    public void Init(ItemInstance inst)
    {
        if (inst == null || inst.IsEmpty || inst.itemDefinition == null)
            return;

        itemId.Value   = inst.itemDefinition.id;
        quantity.Value = inst.quantity;
        level.Value    = inst.level;
    }

    // ================= SYNC → RUNTIME =================

    public override void OnStartClient()
    {
        base.OnStartClient();

        runtimeApplied = false;

        itemId.OnChange   += OnChanged;
        quantity.OnChange += OnChanged;
        level.OnChange    += OnChanged;

        TryApplyRuntime();
    }

    public override void OnStopClient()
    {
        itemId.OnChange   -= OnChanged;
        quantity.OnChange -= OnChanged;
        level.OnChange    -= OnChanged;
    }

    private void OnChanged(string _, string __, bool ___) => TryApplyRuntime();
    private void OnChanged(int _, int __, bool ___)       => TryApplyRuntime();

    private void TryApplyRuntime()
    {
        if (runtimeApplied)
            return;

        if (string.IsNullOrEmpty(itemId.Value) || quantity.Value <= 0)
            return;

        var def = ItemRegistrySO.Instance?.Get(itemId.Value);
        if (def == null)
        {
            Debug.LogError($"[WorldItemNetwork] Item not found: {itemId.Value}");
            return;
        }

        runtimeHolder ??=
            GetComponent<ItemRuntimeHolder>() ??
            gameObject.AddComponent<ItemRuntimeHolder>();

        runtimeHolder.SetInstance(
            new ItemInstance(def, quantity.Value, level.Value)
        );

        runtimeApplied = true;
    }

    // ================= SERVER CONSUME =================

    [Server]
    public void ServerConsume()
    {
        if (!IsPickupAvailable)
            return;

        quantity.Value = 0;
        Despawn();
    }
}
