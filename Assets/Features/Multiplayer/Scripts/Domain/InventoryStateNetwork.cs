using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;

public sealed class InventoryStateNetwork : NetworkBehaviour
{
    private InventoryManager inventory;

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

        // initial snapshot (host / early owner)
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

        // remote owner → запрос полного состояния
        if (IsOwner)
            RequestFullState_Server();
    }

    // ======================================================
    // COMMANDS (CLIENT → SERVER)
    // ======================================================

    [ServerRpc(RequireOwnership = true)]
    public void RequestInventoryCommand(InventoryCommandData cmd)
    {
        if (inventory == null || inventory.Service == null)
            return;

        switch (cmd.Command)
        {
            case InventoryCommand.SelectHotbar:
                inventory.Service.SelectHotbarIndex(cmd.Index);
                break;

            case InventoryCommand.MoveItem:
                inventory.Service.MoveItem(
                    cmd.FromIndex,
                    cmd.FromSection,
                    cmd.ToIndex,
                    cmd.ToSection
                );
                break;

            case InventoryCommand.DropFromSlot:
                HandleDrop(cmd);
                break;

            case InventoryCommand.UpgradeItem:
                HandleUpgrade(cmd);
                break;

            case InventoryCommand.CraftRecipe:
                HandleCraft(cmd);
                break;

            case InventoryCommand.PickupWorldItem:
                HandlePickup(cmd);
                break;
        }
        // state sync пойдёт через OnInventoryChanged
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestFullState_Server()
    {
        ServerOnInventoryChanged();
    }

    // ======================================================
    // SERVER → CLIENT STATE
    // ======================================================

    [Server]
    private void ServerOnInventoryChanged()
    {
        if (inventory == null || inventory.Model == null)
            return;

        var m = inventory.Model;

        // ---------- OWNER : FULL SNAPSHOT ----------
        var owner = Owner;
        if (owner != null)
        {
            var bag = new InventorySlotNet[m.main.Count];
            for (int i = 0; i < m.main.Count; i++)
                bag[i] = ToNet(m.main[i].item);

            var hotbar = new InventorySlotNet[m.hotbar.Count];
            for (int i = 0; i < m.hotbar.Count; i++)
                hotbar[i] = ToNet(m.hotbar[i].item);

            var left  = ToNet(m.leftHand.item);
            var right = ToNet(m.rightHand.item);

            TargetReceiveInventoryState(
                owner,
                bag,
                hotbar,
                left,
                right,
                m.selectedHotbarIndex
            );
        }

        // ---------- OBSERVERS : HANDS ONLY ----------
        ObserversReceiveHands(
            ToNet(m.leftHand.item),
            ToNet(m.rightHand.item)
        );
    }

    [TargetRpc]
    private void TargetReceiveInventoryState(
        NetworkConnection _,
        InventorySlotNet[] bag,
        InventorySlotNet[] hotbar,
        InventorySlotNet left,
        InventorySlotNet right,
        int selectedIndex)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        inventory.ApplyNetState(
            bag,
            hotbar,
            left,
            right,
            selectedIndex
        );
    }

    [ObserversRpc]
    private void ObserversReceiveHands(
        InventorySlotNet left,
        InventorySlotNet right)
    {
        if (inventory == null)
            inventory = GetComponent<InventoryManager>();

        if (inventory == null)
            return;

        // observers знают только экип (для EquipmentManager)
        inventory.ApplyHandsNetState(left, right);
    }

    // ======================================================
    // NET CONVERSION
    // ======================================================

    private static InventorySlotNet ToNet(ItemInstance inst)
    {
        if (inst == null ||
            inst.IsEmpty ||
            inst.itemDefinition == null ||
            inst.quantity <= 0)
        {
            return new InventorySlotNet
            {
                itemId = null,
                quantity = 0,
                level = 0
            };
        }

        return new InventorySlotNet
        {
            itemId = inst.itemDefinition.id,
            quantity = inst.quantity,
            level = inst.level
        };
    }

    // ======================================================
    // HANDLERS (SERVER ONLY)
    // ======================================================

    private void HandleDrop(InventoryCommandData cmd)
    {
        var extracted = inventory.Service.ExtractFromSlot(
            cmd.Section,
            cmd.Index,
            cmd.Amount <= 0 ? int.MaxValue : cmd.Amount
        );

        if (extracted.IsEmpty)
            return;

        WorldItemDropService.I.DropServer(
            extracted,
            cmd.WorldPos,
            cmd.WorldForward
        );
    }

    private void HandleUpgrade(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance.GetRecipeById(cmd.RecipeId);
        if (recipe == null)
            return;

        inventory.Service.ExtractFromSlot(cmd.Section, cmd.Index, 0);
        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var inst = GetSlot(cmd.Section, cmd.Index)?.item;
        if (inst != null)
            inst.level++;
    }

    private void HandleCraft(InventoryCommandData cmd)
    {
         Debug.Log($"[CRAFT] Server craft {cmd.RecipeId}");
        var recipe = RecipeDatabase.Instance.GetRecipeById(cmd.RecipeId);
        if (recipe == null)
        {
            Debug.Log("[CRAFT] Recipe not found");
            return;
        }

        if (!inventory.Service.HasIngredients(recipe.ingredients))
        {
            Debug.Log("[CRAFT] Missing ingredients on SERVER");
            return;
        }

        Debug.Log("[CRAFT] Ingredients OK, crafting...");

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var output = recipe.outputItem;
        bool added;
        if (output.isStackable)
        {
            added = inventory.Service.AddItem(
                new ItemInstance(output, recipe.outputAmount)
            );
        }
        else
        {
            added = true;
            for (int i = 0; i < recipe.outputAmount; i++)
                added &= inventory.Service.AddItem(
                    new ItemInstance(output, 1)
                );
        }

        Debug.Log($"[CRAFT] AddItem result = {added}");
    }

    private void HandlePickup(InventoryCommandData cmd)
    {
        if (!NetworkManager.ServerManager.Objects.Spawned
            .TryGetValue(cmd.WorldItemNetId, out var netObj))
        {
            Debug.LogWarning(
                $"[Pickup] WorldItem {cmd.WorldItemNetId} not found");
            return;
        }

        var worldItem = netObj.GetComponent<WorldItemNetwork>();
        if (worldItem == null || !worldItem.IsPickupAvailable)
            return;

        var def = ItemRegistrySO.Instance.Get(worldItem.ItemId);
        if (def == null)
            return;

        if (!inventory.Service.AddItem(
                new ItemInstance(
                    def,
                    worldItem.Quantity,
                    worldItem.Level)))
            return;

        worldItem.ServerConsume();
    }

    private InventorySlot GetSlot(InventorySection section, int index)
    {
        return section switch
        {
            InventorySection.Bag       => inventory.Model.main[index],
            InventorySection.Hotbar    => inventory.Model.hotbar[index],
            InventorySection.LeftHand  => inventory.Model.leftHand,
            InventorySection.RightHand => inventory.Model.rightHand,
            _ => null
        };
    }
}
