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

    private ItemInstance selectedInstance;
    private UpgradeRecipeSO selectedRecipe;

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
        BuildUpgradeList();
    }

    // ======================================================
    // BUILD LIST
    // ======================================================

    private void BuildUpgradeList()
    {
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        var upgradeRecipes = station.GetRecipes()
            .Where(r => r.recipeType == RecipeType.Upgrade)
            .OfType<UpgradeRecipeSO>()
            .ToArray();

        var allSlots = inventory.Model.GetAllSlots();
        var seen = new HashSet<string>();

        foreach (var slot in allSlots)
        {
            var inst = slot.item;
            if (inst == null) continue;

            var def = inst.itemDefinition;
            if (def == null) continue;

            if (!seen.Add(def.id)) continue;
            if (def.upgrades == null || inst.level >= def.upgrades.Length) continue;

            var recipe = upgradeRecipes.FirstOrDefault(r =>
                r.upgradeBaseItem != null &&
                r.upgradeBaseItem.id == def.id);

            if (recipe == null) continue;

            var btn = Instantiate(upgradeGlowButtonPrefab, recipeListContainer);
            btn.Init(inst, recipe, this);
        }
    }

    // ======================================================
    // UI CALLBACKS
    // ======================================================

    public void OnUpgradeItemSelected(ItemInstance inst, RecipeSO recipeBase)
    {
        var recipe = recipeBase as UpgradeRecipeSO;
        if (recipe == null)
            return;

        selectedInstance = inst;
        selectedRecipe = recipe;

        recipePanel.ShowUpgradeRecipe(inst, recipe);

        // 🔥 ВАЖНО:
        // апгрейд стартует ТОЛЬКО по кнопке
        recipePanel.SetAction(() =>
        {
            if (!processor.IsProcessing)
                processor.BeginUpgrade(selectedRecipe, selectedInstance);
        });
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
