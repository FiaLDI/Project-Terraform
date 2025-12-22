using FishNet.Object;
using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;

public sealed class InventoryStateNetwork : NetworkBehaviour
{
    private InventoryManager inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestInventoryCommand(InventoryCommandData cmd)
    {
        if (inventory == null || inventory.Service == null)
            return;

        Debug.Log($"[INV CMD] {cmd.Command}");

        switch (cmd.Command)
        {
            case InventoryCommand.SelectHotbar:
                HandleSelectHotbar(cmd);
                break;

            case InventoryCommand.MoveItem:
                HandleMove(cmd);
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
    }

    // ================= HANDLERS =================

    private void HandleSelectHotbar(InventoryCommandData cmd)
    {
        inventory.Service.SelectHotbarIndex(cmd.Index);
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

        var slot = inventory.Service
            .ExtractFromSlot(cmd.Section, cmd.Index, 0);

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var inst = GetSlot(cmd.Section, cmd.Index).item;
        inst.level++;
    }

    private InventorySlot GetSlot(InventorySection section, int index)
    {
        return section switch
        {
            InventorySection.Bag => inventory.Model.main[index],
            InventorySection.Hotbar => inventory.Model.hotbar[index],
            InventorySection.LeftHand => inventory.Model.leftHand,
            InventorySection.RightHand => inventory.Model.rightHand,
            _ => null
        };
    }


    private void HandleCraft(InventoryCommandData cmd)
    {
        var recipe = RecipeDatabase.Instance.GetRecipeById(cmd.RecipeId);
        if (recipe == null)
            return;

        if (!inventory.Service.HasIngredients(recipe.ingredients))
            return;

        inventory.Service.ConsumeIngredients(recipe.ingredients);

        var output = recipe.outputItem;
        if (output.isStackable)
        {
            inventory.Service.AddItem(
                new ItemInstance(output, recipe.outputAmount)
            );
        }
        else
        {
            for (int i = 0; i < recipe.outputAmount; i++)
                inventory.Service.AddItem(new ItemInstance(output, 1));
        }
    }

    private void HandlePickup(InventoryCommandData cmd)
    {
        if (!NetworkManager.ServerManager.Objects
                .Spawned.TryGetValue(cmd.WorldItemNetId, out var netObj))
            return;

        var worldItem = netObj.GetComponent<WorldItemNetwork>();
        if (worldItem == null || !worldItem.IsPickupAvailable)
            return;

        var def = ItemRegistrySO.Instance.Get(worldItem.ItemId);
        if (def == null)
            return;

        if (!inventory.Service.AddItem(
                new ItemInstance(def, worldItem.Quantity, worldItem.Level)))
            return;

        worldItem.ServerConsume();
    }

}
