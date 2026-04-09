using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using System.Collections;
using Features.Equipment.UnityIntegration;
using Features.Items.Data;

public sealed class InventoryStateNetwork : NetworkBehaviour
{
    private InventoryManager inventory;
    private bool syncing;
    private int applyCount;

    private InventoryCommandRouter router;

    private const string LOG = "[Inventory]";

    // ======================================================
    // LIFECYCLE
    // ======================================================

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        inventory = GetComponent<InventoryManager>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory != null)
            inventory.OnInventoryChanged += ServerOnInventoryChanged;

        BuildPipelines();

        StartCoroutine(InitialSnapshotRoutine());
    }

    private void BuildPipelines()
    {
        router = new InventoryCommandRouter();

        // ========= COMMON =========
        InventoryCommandPipeline CreateBase(System.Action<InventoryCommandContext> handler)
        {
            var p = new InventoryCommandPipeline();

            p.Add(new InventoryLoggingMiddleware());
            p.Add(new InventoryValidationMiddleware());
            p.Add(new InventoryRateLimitMiddleware());

            p.Add(new InventoryCommandHandlerMiddleware(handler));

            return p;
        }

        // ========= REGISTER =========
        router.Register(InventoryCommand.PickupWorldItem, CreateBase(ctx => HandlePickup(ctx.Command)));
        router.Register(InventoryCommand.MoveItem,        CreateBase(ctx => HandleMove(ctx.Command)));
        router.Register(InventoryCommand.DropFromSlot,    CreateBase(ctx => HandleDrop(ctx.Command)));
        router.Register(InventoryCommand.UpgradeItem,     CreateBase(ctx => HandleUpgrade(ctx.Command)));
        router.Register(InventoryCommand.CraftRecipe,     CreateBase(ctx => HandleCraft(ctx.Command)));
        router.Register(InventoryCommand.GiveReward,      CreateBase(ctx => HandleGiveReward(ctx.Command)));
    }

    [Server]
    private IEnumerator InitialSnapshotRoutine()
    {
        yield return null;
        ServerOnInventoryChanged();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (inventory != null)
            inventory.OnInventoryChanged -= ServerOnInventoryChanged;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (IsOwner)
            RequestFullState_Server();
    }

    // ======================================================
    // COMMAND ENTRY
    // ======================================================

    [ServerRpc(RequireOwnership = true)]
    public void RequestInventoryCommand(InventoryCommandData cmd)
    {
        var ctx = new InventoryCommandContext
        {
            Command = cmd,
            Inventory = inventory,
            Sender = Owner,
            Owner = this
        };

        router.Execute(ctx);
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestFullState_Server()
    {
        ServerOnInventoryChanged();
    }

    // ======================================================
    // SERVER → CLIENT SYNC
    // ======================================================

    [Server]
    private void ServerOnInventoryChanged()
    {
        if (syncing || inventory == null || inventory.Model == null)
            return;

        syncing = true;

        try
        {
            var m = inventory.Model;

            var owner = Owner;
            if (owner != null)
            {
                var bag = new InventorySlotNet[m.main.Count];

                for (int i = 0; i < m.main.Count; i++)
                    bag[i] = ToNet(m.main[i].item);

                TargetReceiveInventoryState(owner, bag,
                    ToNet(m.leftHand.item),
                    ToNet(m.rightHand.item));
            }

            ObserversReceiveHands(
                ToNet(m.leftHand.item),
                ToNet(m.rightHand.item));
        }
        finally
        {
            syncing = false;
        }
    }

    [TargetRpc]
    private void TargetReceiveInventoryState(NetworkConnection _, InventorySlotNet[] bag, InventorySlotNet left, InventorySlotNet right)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;

        try
        {
            inventory.ApplyNetState(bag, left, right);

            GetComponent<EquipmentManager>()?.EquipFromInventory();
        }
        finally
        {
            syncing = false;
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversReceiveHands(InventorySlotNet left, InventorySlotNet right)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;

        try
        {
            inventory.ApplyHandsNetState(left, right);
            GetComponent<EquipmentManager>()?.EquipFromInventory();
        }
        finally
        {
            syncing = false;
        }
    }

    // ======================================================
    // HANDLERS
    // ======================================================

    private void HandleGiveReward(InventoryCommandData cmd)
    {
        if (!TryGetItemDef(cmd.RewardItemId, out var def))
            return;

        inventory.Service.AddItem(
            new ItemInstance(def, cmd.RewardAmount, cmd.RewardLevel)
        );
    }

    private void HandleMove(InventoryCommandData cmd)
    {
        inventory.Service.MoveItem(
            cmd.FromIndex, cmd.FromSection,
            cmd.ToIndex, cmd.ToSection);
    }

    private void HandleUpgrade(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance?.GetRecipeById(cmd.RecipeId);
        if (recipe == null) return;

        if (!inventory.Service.HasIngredients(recipe.ingredients)) return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var slot = GetSlot(cmd.Section, cmd.Index);
        if (slot != null && !slot.item.IsEmpty)
            slot.item.level++;
    }

    private void HandleCraft(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance?.GetRecipeById(cmd.RecipeId);
        if (recipe == null) return;

        if (!inventory.Service.HasIngredients(recipe.ingredients)) return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        inventory.Service.AddItem(
            new ItemInstance(recipe.outputItem, recipe.outputAmount));
    }

    private void HandlePickup(InventoryCommandData cmd)
    {
        if (!NetworkManager.ServerManager.Objects.Spawned
            .TryGetValue((int)cmd.WorldItemNetId, out var netObj))
        {
            Debug.LogWarning("[Pickup] WorldItem not found");
            return;
        }

        var worldItem = netObj.GetComponent<WorldItemNetwork>();
        if (worldItem == null)
        {
            Debug.LogWarning("[Pickup] No WorldItemNetwork");
            return;
        }

        var def = ItemRegistrySO.Instance?.Get(worldItem.ItemId);
        if (def == null)
            return;

        var inst = new ItemInstance(def, worldItem.Quantity, worldItem.Level);

        if (!inventory.Service.AddItem(inst))
        {
            Debug.LogWarning("[Pickup] Inventory full");
            return;
        }
        
        worldItem.ServerConsume();
    }

    private void HandleDrop(InventoryCommandData cmd)
    {
        var extracted = inventory.Service.ExtractFromSlot(cmd.Section, cmd.Index, cmd.Amount);

        var dropService = WorldItemDropService.I;
        if (dropService == null)
            return;

        dropService.DropServer(extracted, cmd.WorldPos, cmd.WorldForward);
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private bool TryGetItemDef(string id, out Item def)
    {
        def = ItemRegistrySO.Instance?.Get(id);
        return def != null;
    }

    private static InventorySlotNet ToNet(ItemInstance inst)
    {
        if (inst == null || inst.IsEmpty)
            return default;

        return new InventorySlotNet
        {
            itemId = inst.itemDefinition.id,
            quantity = inst.quantity,
            level = inst.level
        };
    }

    private InventorySlot GetSlot(InventorySection section, int index)
    {
        if (inventory == null || inventory.Model == null)
            return null;

        return section switch
        {
            InventorySection.Bag => inventory.Model.main[index],
            InventorySection.LeftHand => inventory.Model.leftHand,
            InventorySection.RightHand => inventory.Model.rightHand,
            _ => null
        };
    }

    [Server]
    public void ExecuteCommandServer(InventoryCommandData cmd)
    {
        var ctx = new InventoryCommandContext
        {
            Command = cmd,
            Inventory = inventory,
            Sender = Owner, // или null если системный вызов
            Owner = this
        };

        router.Execute(ctx);
    }

    [Server]
    public void ForceSync()
    {
        ServerOnInventoryChanged();
    }
}
