using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Features.Inventory;
using Features.Items.Domain;
using Features.Inventory.Domain;
using Features.Items.Data;
using Features.Inventory.UnityIntegration;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;
    private InventoryManager inventory;

    [Header("Upgrade UI")]
    [SerializeField] private UpgradeGlowButtonUI upgradeGlowButtonPrefab;

    private ItemInstance     selectedInstance;
    private UpgradeRecipeSO  selectedRecipe;
    private InventorySlotRef selectedSlot;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(UpgradeStation station, UpgradeProcessor processor, IInventoryContext inventory)
    {
        this.station   = station;
        this.processor = processor;
        this.inventory = inventory as InventoryManager; // (1)

        recipePanel.Init(inventory);

        processor.OnStart    += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;

        if (this.inventory != null)
            this.inventory.OnInventoryChanged += OnInventoryChanged;
    }

    // ======================================================
    // UI LIFECYCLE
    // ======================================================

    public override void Show()
    {
        base.Show();
        OnOpenUI();
    }

    private void OnOpenUI()
    {
        ClearSelection();
        BuildUpgradeList();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    // ======================================================
    // BUILD LIST
    // ======================================================

    private void BuildUpgradeList()
    {
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        if (inventory == null || station == null)
            return;

        var upgradeRecipes = station.GetRecipes()
            .Where(r => r.recipeType == RecipeType.Upgrade)
            .OfType<UpgradeRecipeSO>()
            .ToArray();

        var seen = new HashSet<string>();

        // ---- BAG ----
        for (int i = 0; i < inventory.Model.main.Count; i++)
        {
            var slot = inventory.Model.main[i];
            AddSlotIfUpgradable(
                slot.item,
                new InventorySlotRef(InventorySection.Bag, i),
                upgradeRecipes,
                seen);
        }

        // ---- LEFT HAND ----
        AddSlotIfUpgradable(
            inventory.Model.leftHand.item,
            new InventorySlotRef(InventorySection.LeftHand, 0),
            upgradeRecipes,
            seen);

        // ---- RIGHT HAND ----
        AddSlotIfUpgradable(
            inventory.Model.rightHand.item,
            new InventorySlotRef(InventorySection.RightHand, 0),
            upgradeRecipes,
            seen);
    }

    private void AddSlotIfUpgradable(
        ItemInstance inst,
        InventorySlotRef slotRef,
        UpgradeRecipeSO[] upgradeRecipes,
        HashSet<string> seen)
    {
        if (inst == null || inst.IsEmpty)
            return;

        var def = inst.itemDefinition;
        if (def == null || def.upgrades == null || def.upgrades.Length == 0)
            return;

        if (inst.level >= def.upgrades.Length)
            return;

        // один пункт на тип предмета
        if (!seen.Add(def.id))
            return;

        var recipe = upgradeRecipes.FirstOrDefault(r =>
            r.upgradeBaseItem != null &&
            r.upgradeBaseItem.id == def.id);

        if (recipe == null)
            return;

        var btn = Instantiate(
            upgradeGlowButtonPrefab,
            recipeListContainer);

        // передаём и слот, и инстанс
        btn.Init(inst, recipe, this, slotRef);
    }

    // ======================================================
    // UI CALLBACKS
    // ======================================================
    public void OnUpgradeItemSelected(
        ItemInstance inst,
        RecipeSO recipeBase,
        InventorySlotRef slotRef)
    {
        var recipe = recipeBase as UpgradeRecipeSO;
        if (recipe == null)
            return;

        selectedInstance = inst;
        selectedRecipe   = recipe;
        selectedSlot     = slotRef;

        Debug.Log($"[UpgradeUI] Selected item={inst.itemDefinition.id} lvl={inst.level}, slot={slotRef.Section}[{slotRef.Index}], recipe={recipe.recipeId}");

        recipePanel.ShowUpgradeRecipe(inst, recipe, slotRef);

        recipePanel.SetAction(() =>
        {
            Debug.Log($"[UpgradeUI] Action pressed for item={selectedInstance.itemDefinition.id} lvl={selectedInstance.level}");

            if (selectedInstance == null || selectedRecipe == null)
                return;

            var def = selectedInstance.itemDefinition;
            if (def == null || def.upgrades == null)
                return;

            if (selectedInstance.level >= def.upgrades.Length)
            {
                Debug.Log("[UpgradeUI] Already max level, cancel");
                return;
            }

            if (!processor.IsProcessing)
            {
                Debug.Log($"[UpgradeUI] BeginUpgrade send, slot={selectedSlot.Section}[{selectedSlot.Index}]");
                processor.BeginUpgrade(
                    selectedRecipe,
                    selectedInstance,
                    selectedSlot);
            }
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
        recipePanel.RefreshUpgradeInfo();
        BuildUpgradeList();
    }

    // ======================================================
    // HELPERS
    // ======================================================

    private void ClearSelection()
    {
        selectedInstance = null;
        selectedRecipe   = null;
        selectedSlot     = default;
        recipePanel.Clear();
    }

    public override void Open()
    {
        base.Open();
    }

    private void OnInventoryChanged()
    {
        if (gameObject.activeInHierarchy)
        {
            BuildUpgradeList();

            recipePanel.RefreshUpgradeInfo();
        }
    }
}
