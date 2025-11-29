using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;

    [Header("Upgrade UI")]
    [SerializeField] private UpgradeItemButtonUI upgradeItemButtonPrefab;

    private readonly List<(Item item, RecipeSO recipe)> candidates = new();

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
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        candidates.Clear();

        var upgradeRecipes = station.GetRecipes();

        var allSlots = InventoryManager.Instance.GetAllSlots();

        var seenItems = new HashSet<Item>(); 

        foreach (var slot in allSlots)
        {
            Item item = slot.ItemData;

            if (item == null)
                continue;

            if (!seenItems.Add(item))
            {
                continue;
            }

            if (item.upgrades == null || item.upgrades.Length == 0)
            {
                continue;
            }

            int targetLevel = item.currentLevel + 1;

            if (targetLevel >= item.upgrades.Length)
            {
                Debug.Log($"[UPGRADE] Item {item.name} is MAX LEVEL ({item.currentLevel}).");
                continue;
            }

            RecipeSO recipe = upgradeRecipes.FirstOrDefault(r =>
                r.recipeType == RecipeType.Upgrade &&
                r.upgradeBaseItem == item &&
                r.upgradeTargetLevel == targetLevel &&
                r.requiresUpgradeStation
            );

            if (recipe == null)
            {
                continue;
            }

            candidates.Add((item, recipe));

            var btn = Instantiate(upgradeItemButtonPrefab, recipeListContainer);
            btn.Init(item, recipe, this);
        }
    }

    public void OnUpgradeItemSelected(Item item, RecipeSO recipe)
    {
        recipePanel.ShowUpgradeRecipe(item, recipe);

        recipePanel.SetAction(() =>
        {
            var slot = InventoryManager.Instance.FindFirstSlotWithItem(item);
            if (slot == null)
            {
                Debug.LogWarning($"[UpgradeStationUI] Не найден слот с {item.itemName}");
                return;
            }

            processor.BeginUpgrade(recipe, slot);
        });

        recipePanel.ShowMissingIngredients(recipe);
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
    }

    public void OnOpenUI()
    {
        BuildUpgradeList();
    }

}
