using System.Collections.Generic;
using System.Linq;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Data;
using Features.Items.Domain;

public readonly struct UpgradeCandidate
{
    public UpgradeCandidate(ItemInstance instance, UpgradeRecipeSO recipe, InventorySlotRef slotRef)
    {
        Instance = instance;
        Recipe = recipe;
        SlotRef = slotRef;
    }

    public ItemInstance Instance { get; }
    public UpgradeRecipeSO Recipe { get; }
    public InventorySlotRef SlotRef { get; }
}

public sealed class UpgradeCandidateBuilder
{
    public List<UpgradeCandidate> Build(InventoryManager inventory, UpgradeRecipeSO[] recipes)
    {
        var candidates = new List<UpgradeCandidate>();
        if (inventory?.Model == null || recipes == null || recipes.Length == 0)
            return candidates;

        Dictionary<string, UpgradeRecipeSO> recipesByItemId = recipes
            .Where(recipe => recipe != null && recipe.upgradeBaseItem != null)
            .GroupBy(recipe => recipe.upgradeBaseItem.id)
            .ToDictionary(group => group.Key, group => group.First());

        var seen = new HashSet<string>();

        for (int i = 0; i < inventory.Model.main.Count; i++)
        {
            AddCandidate(
                inventory.Model.main[i].item,
                new InventorySlotRef(InventorySection.Bag, i),
                recipesByItemId,
                seen,
                candidates);
        }

        AddCandidate(
            inventory.Model.leftHand.item,
            new InventorySlotRef(InventorySection.LeftHand, 0),
            recipesByItemId,
            seen,
            candidates);

        AddCandidate(
            inventory.Model.rightHand.item,
            new InventorySlotRef(InventorySection.RightHand, 0),
            recipesByItemId,
            seen,
            candidates);

        return candidates;
    }

    private void AddCandidate(
        ItemInstance inst,
        InventorySlotRef slotRef,
        Dictionary<string, UpgradeRecipeSO> recipesByItemId,
        HashSet<string> seen,
        List<UpgradeCandidate> candidates)
    {
        if (inst == null || inst.IsEmpty)
            return;

        Item definition = inst.itemDefinition;
        if (definition == null || definition.upgrades == null || definition.upgrades.Length == 0)
            return;

        if (inst.level >= definition.upgrades.Length)
            return;

        if (!seen.Add(definition.id))
            return;

        if (!recipesByItemId.TryGetValue(definition.id, out UpgradeRecipeSO recipe))
            return;

        candidates.Add(new UpgradeCandidate(inst, recipe, slotRef));
    }
}
