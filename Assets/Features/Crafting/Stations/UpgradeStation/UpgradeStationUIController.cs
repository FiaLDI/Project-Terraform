using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;

    [Header("Upgrade UI")]
    [SerializeField] private UpgradeGlowButtonUI upgradeGlowButtonPrefab;

    private readonly List<(Item item, UpgradeRecipeSO recipe)> candidates = new();

    public void Init(UpgradeStation station, UpgradeProcessor processor)
    {
        this.station = station;
        this.processor = processor;

        BuildUpgradeList();

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
    }


    private void BuildUpgradeList()
    {
        Debug.Log("<color=cyan>[UPGRADE] BUILD LIST</color>");

        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        candidates.Clear();

        // ---- 1) Берём рецепты только для upgrade station ----
        var upgradeRecipes = station.GetRecipes()
            .Where(r => r.recipeType == RecipeType.Upgrade)
            .OfType<UpgradeRecipeSO>()
            .ToArray();

        // ---- 2) Сканируем предметы игрока ----
        var allSlots = InventoryManager.Instance.GetAllSlots();
        var seenItems = new HashSet<Item>();

        foreach (var slot in allSlots)
        {
            Item item = slot.ItemData;
            if (item == null) continue;

            if (!seenItems.Add(item))
                continue;

            if (item.upgrades == null || item.upgrades.Length == 0)
                continue;

            int nextLevel = item.currentLevel + 1;

            if (nextLevel > item.upgrades.Length)
            {
                Debug.Log($"[UPGRADE] {item.itemName} MAX LEVEL");
                continue;
            }

            // ---- 3) Автоматический поиск рецепта ----
            UpgradeRecipeSO recipe = upgradeRecipes.FirstOrDefault(r =>
                r.upgradeBaseItem != null &&
                r.upgradeBaseItem.id == item.id
            );

            if (recipe == null)
            {
                Debug.Log($"[UPGRADE] No recipe found for {item.itemName}");
                continue;
            }

            candidates.Add((item, recipe));

            // ---- UI button ----
            var btn = Instantiate(upgradeGlowButtonPrefab, recipeListContainer);
            btn.Init(item, recipe, this);
        }

        Debug.Log($"<color=yellow>[UPGRADE] Visible = {candidates.Count}</color>");
    }


    public void OnUpgradeItemSelected(Item item, RecipeSO recipeBase)
    {
        var recipe = recipeBase as UpgradeRecipeSO;
        if (recipe == null) return;

        ShowUpgradePanel(item, recipe);
    }

    private void ShowUpgradePanel(Item item, UpgradeRecipeSO recipe)
    {
        recipePanel.ShowUpgradeRecipe(item, recipe);
        recipePanel.ShowMissingIngredients(recipe);

        recipePanel.SetAction(() =>
        {
            var slot = InventoryManager.Instance.FindFirstSlotWithItem(item);
            if (slot != null)
                processor.BeginUpgrade(recipe, slot);
        });
    }


    private void HandleStart(Item item)
    {
        recipePanel.StartProgress();
    }

    private void HandleProgress(float t)
    {
        recipePanel.UpdateProgress(t);
    }

    private void HandleComplete(Item item)
    {
        recipePanel.ProcessComplete();

        BuildUpgradeList();

        var recipe = station.GetRecipes()
            .Where(r => r.recipeType == RecipeType.Upgrade)
            .OfType<UpgradeRecipeSO>()
            .FirstOrDefault(r =>
                r.upgradeBaseItem != null &&
                r.upgradeBaseItem.id == item.id
            );

        if (recipe != null)
            ShowUpgradePanel(item, recipe);
        else
            recipePanel.ClosePanel();
    }


    public void OnOpenUI()
    {
        BuildUpgradeList();
    }
}
