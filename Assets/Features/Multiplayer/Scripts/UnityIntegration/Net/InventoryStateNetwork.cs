using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Data;
using Features.Items.Domain;
using Features.Equipment.UnityIntegration;
using Multiplayer.Domain;

public sealed class InventoryStateNetwork : NetworkBehaviour
{
    private InventoryManager inventory;

    private bool syncing;
    private bool requestedInitial;
    private bool initialized;

    private InventoryCommandRouter router;

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

        var root = ServerCompositionRoot.I;
        var session = root?.Sessions?.GetSessionByClient(Owner.ClientId);

        if (session != null && session.HasInventory)
        {
            ApplyInventoryFromSession(session);
            initialized = true;
            return;
        }

        StartCoroutine(InitialRequestRoutine());
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
    // INITIAL SYNC
    // ======================================================

    private IEnumerator InitialRequestRoutine()
    {
        yield return null;

        float timeout = 5f;
        float elapsed = 0f;

        while (!initialized && elapsed < timeout)
        {
            if (!requestedInitial && Owner != null)
            {
                TargetRequestInitialInventory(Owner);
                requestedInitial = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!initialized)
        {
            Debug.LogWarning("[Inventory] No initial data received, fallback sync");
            ServerOnInventoryChanged();
        }
    }

    [TargetRpc]
    private void TargetRequestInitialInventory(NetworkConnection conn)
    {
        var net = GetComponent<InventoryNetwork>();
        net?.SendInitialInventoryToServer();
    }

    // ======================================================
    // APPLY
    // ======================================================

    [Server]
    public void ApplyInitialInventory(InventorySaveData data)
    {
        if (initialized)
            return;

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null || data == null)
            return;

        inventory.LoadFromSave(data);

        initialized = true;
        ServerOnInventoryChanged();
    }

    [Server]
    public void ApplyInventoryFromSession(PlayerSession session)
    {
        if (initialized || session == null)
            return;

        var data = session.InventoryData;
        if (data == null)
            return;

        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        inventory.LoadFromSave(data);

        initialized = true;
        ServerOnInventoryChanged();
    }

    // ======================================================
    // COMMAND ENTRY
    // ======================================================

    [ServerRpc(RequireOwnership = true)]
    public void RequestInventoryCommand(InventoryCommandData cmd)
    {
        if (inventory == null || router == null)
            return;

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
    // SYNC
    // ======================================================

    [Server]
    private void ServerOnInventoryChanged()
    {
        if (syncing || inventory == null || inventory.Model == null)
            return;

        syncing = true;

        try
        {
            var root = ServerCompositionRoot.I;
            var session = root?.Sessions?.GetSessionByClient(Owner.ClientId);

            if (session != null)
                session.SetInventory(inventory.BuildSaveData());

            var m = inventory.Model;

            if (Owner != null)
            {
                var bag = new InventorySlotNet[m.main.Count];

                for (int i = 0; i < m.main.Count; i++)
                    bag[i] = ToNet(m.main[i].item);

                TargetReceiveInventoryState(
                    Owner,
                    bag,
                    ToNet(m.activeSlot0.item),
                    ToNet(m.activeSlot1.item),
                    ToNet(m.activeSlot2.item),
                    m.ActiveSlotIndex
                );
            }

            ObserversReceiveActiveSlots(
                ToNet(m.activeSlot0.item),
                ToNet(m.activeSlot1.item),
                ToNet(m.activeSlot2.item),
                m.ActiveSlotIndex
            );
        }
        finally
        {
            syncing = false;
        }
    }

    [TargetRpc]
    private void TargetReceiveInventoryState(
        NetworkConnection _,
        InventorySlotNet[] bag,
        InventorySlotNet active0,
        InventorySlotNet active1,
        InventorySlotNet active2,
        int activeSlotIndex)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;

        try
        {
            inventory.ApplyNetState(bag, active0, active1, active2, activeSlotIndex);
            GetComponent<EquipmentManager>()?.EquipFromInventory();
        }
        finally
        {
            syncing = false;
        }
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversReceiveActiveSlots(
        InventorySlotNet active0,
        InventorySlotNet active1,
        InventorySlotNet active2,
        int activeSlotIndex)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        syncing = true;

        try
        {
            inventory.ApplyActiveSlotsNetState(active0, active1, active2, activeSlotIndex);
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

    private void BuildPipelines()
    {
        router = new InventoryCommandRouter();

        InventoryCommandPipeline CreateBase(System.Action<InventoryCommandContext> handler)
        {
            var p = new InventoryCommandPipeline();

            p.Add(new InventoryLoggingMiddleware());
            p.Add(new InventoryValidationMiddleware());
            p.Add(new InventoryRateLimitMiddleware());
            p.Add(new InventoryCommandHandlerMiddleware(handler));

            return p;
        }

        router.Register(InventoryCommand.PickupWorldItem, CreateBase(ctx => HandlePickup(ctx.Command)));
        router.Register(InventoryCommand.MoveItem, CreateBase(ctx => HandleMove(ctx.Command)));
        router.Register(InventoryCommand.SetActiveSlot, CreateBase(ctx => HandleSetActiveSlot(ctx.Command)));
        router.Register(InventoryCommand.DropFromSlot, CreateBase(ctx => HandleDrop(ctx.Command)));
        router.Register(InventoryCommand.UpgradeItem, CreateBase(ctx => HandleUpgrade(ctx.Command)));
        router.Register(InventoryCommand.CraftRecipe, CreateBase(ctx => HandleCraft(ctx.Command)));
        router.Register(InventoryCommand.GiveReward, CreateBase(ctx => HandleGiveReward(ctx.Command)));
    }

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
            cmd.FromIndex,
            cmd.FromSection,
            cmd.ToIndex,
            cmd.ToSection
        );
    }

    private void HandleSetActiveSlot(InventoryCommandData cmd)
    {
        if (inventory == null)
            return;

        int clamped = InventoryModel.ClampActiveSlotIndex(cmd.ActiveSlotIndex);
        inventory.SetActiveSlotIndex(clamped);
    }

    private void HandleUpgrade(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance?.GetRecipeById(cmd.RecipeId);
        if (recipe == null)
            return;

        if (!inventory.Service.HasIngredients(recipe.ingredients))
            return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var slot = GetSlot(cmd.Section, cmd.Index);
        if (slot != null && !slot.item.IsEmpty)
        {
            slot.item.level++;
            inventory.MarkDirty();
        }
    }

    private void HandleCraft(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance?.GetRecipeById(cmd.RecipeId);
        if (recipe == null)
            return;

        if (!inventory.Service.HasIngredients(recipe.ingredients))
            return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        inventory.Service.AddItem(
            new ItemInstance(recipe.outputItem, recipe.outputAmount)
        );
    }

    private void HandlePickup(InventoryCommandData cmd)
    {
        if (!NetworkManager.ServerManager.Objects.Spawned.TryGetValue((int)cmd.WorldItemNetId, out var netObj))
            return;

        var worldItem = netObj.GetComponent<WorldItemNetwork>();
        if (worldItem == null)
            return;

        var def = ItemRegistrySO.Instance?.Get(worldItem.ItemId);
        if (def == null)
            return;

        var inst = new ItemInstance(def, worldItem.Quantity, worldItem.Level);

        if (!inventory.Service.AddItem(inst))
            return;

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
            InventorySection.ActiveSlot0 => inventory.Model.activeSlot0,
            InventorySection.ActiveSlot1 => inventory.Model.activeSlot1,
            InventorySection.ActiveSlot2 => inventory.Model.activeSlot2,
            _ => null
        };
    }

    [Server]
    public void ExecuteCommandServer(InventoryCommandData cmd)
    {
        if (inventory == null || router == null)
            return;

        var ctx = new InventoryCommandContext
        {
            Command = cmd,
            Inventory = inventory,
            Sender = Owner,
            Owner = this
        };

        router.Execute(ctx);
    }
}

