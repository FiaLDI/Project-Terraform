using System.Linq;
using Features.Inventory;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Player;
using UnityEngine;
using UnityEngine.UI;

public sealed class UpgradeStationUI : PlayerBoundStationUI
{
    [SerializeField] private UpgradeProcessor processor;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private RecipePanelUI recipePanel;
    [SerializeField] private UpgradeGlowButtonUI upgradeGlowButtonPrefab;

    private readonly UpgradeCandidateBuilder candidateBuilder = new();

    private InventoryManager inventory;
    private UpgradeRecipeSO[] upgradeRecipes;
    private ItemInstance selectedInstance;
    private UpgradeRecipeSO selectedRecipe;
    private InventorySlotRef selectedSlot;
    private bool initialized;

    protected override void OnPlayerBound(GameObject player)
    {
        if (initialized)
            return;

        if (processor == null || recipePanel == null)
        {
            Debug.LogError("[UpgradeStationUI] Missing required references", this);
            return;
        }

        IInventoryContext localInventory = LocalPlayerContext.Inventory;
        if (localInventory == null)
        {
            Debug.LogError("[UpgradeStationUI] Local inventory not available", this);
            return;
        }

        inventory = localInventory as InventoryManager;
        if (inventory == null)
        {
            Debug.LogError("[UpgradeStationUI] Local inventory is not InventoryManager", this);
            return;
        }

        processor.Init(localInventory);
        recipePanel.Init(localInventory);
        upgradeRecipes = LoadUpgradeRecipes();

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
        inventory.OnInventoryChanged += OnInventoryChanged;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        initialized = true;
    }

    public override void Show()
    {
        base.Show();
        ClearSelection();
        BuildUpgradeList();
    }

    private void OnDestroy()
    {
        if (processor != null)
        {
            processor.OnStart -= HandleStart;
            processor.OnProgress -= HandleProgress;
            processor.OnComplete -= HandleComplete;
        }

        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private void BuildUpgradeList()
    {
        if (recipeListContainer == null)
            return;

        foreach (Transform child in recipeListContainer)
            Destroy(child.gameObject);

        if (inventory == null)
            return;

        if (upgradeRecipes.Length == 0)
            return;

        foreach (UpgradeCandidate candidate in candidateBuilder.Build(inventory, upgradeRecipes))
        {
            UpgradeGlowButtonUI button = Instantiate(upgradeGlowButtonPrefab, recipeListContainer);
            button.Init(candidate.Instance, candidate.Recipe, this, candidate.SlotRef);
        }
    }

    public void OnUpgradeItemSelected(
        ItemInstance inst,
        RecipeSO recipeBase,
        InventorySlotRef slotRef)
    {
        UpgradeRecipeSO recipe = recipeBase as UpgradeRecipeSO;
        if (recipe == null)
            return;

        selectedInstance = inst;
        selectedRecipe = recipe;
        selectedSlot = slotRef;

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
                processor.BeginUpgrade(selectedRecipe, selectedInstance, selectedSlot);
            }
        });
        recipePanel.SetCancelAction(CancelProcessing);
    }

    public override void Hide()
    {
        CancelProcessing();
        base.Hide();
    }

    public override void Close()
    {
        CancelProcessing();
        base.Close();
    }

    private void HandleStart(ItemInstance inst)
        => recipePanel.StartProgress();

    private void HandleProgress(float progress)
        => recipePanel.UpdateProgress(progress);

    private void HandleComplete(ItemInstance inst)
    {
        recipePanel.ProcessComplete();
        recipePanel.RefreshIngredients();
        recipePanel.RefreshUpgradeInfo();
        BuildUpgradeList();
    }

    private void OnInventoryChanged()
    {
        if (!IsVisible)
            return;

        BuildUpgradeList();
        recipePanel.RefreshUpgradeInfo();
    }

    private void ClearSelection()
    {
        CancelProcessing();
        selectedInstance = null;
        selectedRecipe = null;
        selectedSlot = default;
        recipePanel.Clear();
    }

    private UpgradeRecipeSO[] LoadUpgradeRecipes()
    {
        RecipeSO[] recipes = RecipeDatabase.Instance?.GetForUpgrade();

        if (recipes == null)
        {
            Debug.LogError("[UpgradeStationUI] Upgrade recipes are not available", this);
            return new UpgradeRecipeSO[0];
        }

        return recipes
            .OfType<UpgradeRecipeSO>()
            .ToArray();
    }

    public override void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);
    }

    private void CancelProcessing()
    {
        if (processor != null && processor.IsProcessing)
            processor.Cancel();

        if (recipePanel != null)
            recipePanel.ResetProgress();
    }
}
