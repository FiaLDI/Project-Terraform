using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Features.Inventory;
using Features.Items.Domain;
using Features.Inventory.Domain;
using Features.Items.Data;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;
    private IInventoryContext inventory;

    [Header("Upgrade UI")]
    [SerializeField] private UpgradeGlowButtonUI upgradeGlowButtonPrefab;

    private readonly List<(ItemInstance inst, UpgradeRecipeSO recipe)> candidates = new();

    // ======================================================
    // INIT
    // ======================================================

    public void Init(
        UpgradeStation station,
        UpgradeProcessor processor,
        IInventoryContext inventory)
    {
        this.station = station;
        this.processor = processor;
        this.inventory = inventory;

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
    }

    // ======================================================
    // UI OPEN
    // ======================================================

    public void OnOpenUI()
    {
        if (inventory == null)
        {
            Debug.LogError($"{name}: InventoryContext is NULL");
            return;
        }

        BuildUpgradeList();
    }

    // ======================================================
    // BUILD LIST
    // ======================================================

    private void BuildUpgradeList()
    {
        if (recipeListContainer == null)
        {
            Debug.LogError($"{name}: recipeListContainer is NULL");
            return;
        }

        if (upgradeGlowButtonPrefab == null)
        {
            Debug.LogError($"{name}: upgradeGlowButtonPrefab is NULL");
            return;
        }

        if (station == null)
        {
            Debug.LogError($"{name}: station is NULL");
            return;
        }

        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        candidates.Clear();

        var upgradeRecipes = station.GetRecipes()
            .Where(r => r.recipeType == RecipeType.Upgrade)
            .OfType<UpgradeRecipeSO>()
            .ToArray();

        var allSlots = inventory.Model.GetAllSlots();
        var seenDefinitions = new HashSet<string>();

        foreach (var slot in allSlots)
        {
            var inst = slot.item;
            if (inst == null)
                continue;

            var def = inst.itemDefinition;
            if (def == null)
                continue;

            if (!seenDefinitions.Add(def.id))
                continue;

            if (def.upgrades == null || def.upgrades.Length == 0)
                continue;

            if (inst.level + 1 > def.upgrades.Length)
                continue;

            var recipe = upgradeRecipes.FirstOrDefault(r =>
                r.upgradeBaseItem != null &&
                r.upgradeBaseItem.id == def.id);

            if (recipe == null)
                continue;

            candidates.Add((inst, recipe));

            var btn = Instantiate(upgradeGlowButtonPrefab, recipeListContainer);
            btn.Init(inst, recipe, this);
        }
    }

    // ======================================================
    // UI CALLBACKS
    // ======================================================

    public void OnUpgradeItemSelected(ItemInstance inst, RecipeSO recipeBase)
    {
        ShowUpgradePanel(inst, recipeBase as UpgradeRecipeSO);
    }

    private void ShowUpgradePanel(ItemInstance inst, UpgradeRecipeSO recipe)
    {
        if (recipe == null)
            return;

        recipePanel.ShowUpgradeRecipe(inst, recipe);

        var pos = inventory.Service.FindSlot(inst);
        if (pos.HasValue)
        {
            processor.BeginUpgrade(recipe, inst);
        }
    }

    // ======================================================
    // PROCESSOR EVENTS
    // ======================================================

    private void HandleStart(ItemInstance inst)
        => recipePanel.StartProgress();

    private void HandleProgress(float t)
        => recipePanel.UpdateProgress(t);

    private void HandleComplete(ItemInstance inst)
    {
        recipePanel.ProcessComplete();
        recipePanel.RefreshIngredients();

        BuildUpgradeList();
    }
}
