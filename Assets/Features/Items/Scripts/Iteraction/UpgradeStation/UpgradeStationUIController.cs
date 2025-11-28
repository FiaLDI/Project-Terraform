using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;

    [Header("Upgrade UI")]
    [SerializeField] private UpgradeItemButtonUI upgradeItemButtonPrefab;

    // (item, recipe) пары подходящих улучшений
    private readonly List<(Item item, RecipeSO recipe)> candidates = new();

    public void Init(UpgradeStation station, UpgradeProcessor processor)
    {
        this.station = station;
        this.processor = processor;

        BuildUpgradeList(); // ← самое главное

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
    }

    // ============================================================
    //   СТРОИМ СПИСОК "Drill (0/3)", "Shotgun (1/4)", ...
    // ============================================================
    private void BuildUpgradeList()
    {
        Debug.Log("<color=#00FFFF>[UPGRADE] Rebuilding Upgrade List...</color>");

        // очистить содержимое панели
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        candidates.Clear();

        var upgradeRecipes = station.GetRecipes();
        Debug.Log($"[UPGRADE] Recipes in DB: {upgradeRecipes.Length}");

        var allSlots = InventoryManager.Instance.GetAllSlots();

        var seenItems = new HashSet<Item>(); 

        foreach (var slot in allSlots)
        {
            Item item = slot.ItemData;

            if (item == null)
                continue;

            Debug.Log($"[UPGRADE] Checking inventory slot item: {item.itemName}, lvl={item.currentLevel}");

            // предмет уже встречался?
            if (!seenItems.Add(item))
            {
                Debug.Log($"[UPGRADE] Skipping duplicate instance of {item.name}");
                continue;
            }

            // есть апгрейды?
            if (item.upgrades == null || item.upgrades.Length == 0)
            {
                Debug.Log($"[UPGRADE] Item {item.name} has NO upgrades defined.");
                continue;
            }

            // есть следующий уровень?
            int targetLevel = item.currentLevel + 1;

            if (targetLevel >= item.upgrades.Length)
            {
                Debug.Log($"[UPGRADE] Item {item.name} is MAX LEVEL ({item.currentLevel}).");
                continue;
            }

            Debug.Log($"[UPGRADE] Target upgrade level = {targetLevel}");

            // ИЩЕМ ПОДХОДЯЩИЙ РЕЦЕПТ
            RecipeSO recipe = upgradeRecipes.FirstOrDefault(r =>
                r.recipeType == RecipeType.Upgrade &&
                r.upgradeBaseItem == item &&
                r.upgradeTargetLevel == targetLevel &&
                r.requiresUpgradeStation
            );

            if (recipe == null)
            {
                Debug.Log(
                    $"<color=#FF4444>[UPGRADE] NO RECIPE FOUND:</color> " +
                    $"BaseItem={item.name}, NeedLevel={targetLevel}");
                continue;
            }

            Debug.Log($"<color=#00FF00>[UPGRADE] FOUND RECIPE:</color> {recipe.recipeId} for {item.name} → L{targetLevel}");

            // добавляем в список
            candidates.Add((item, recipe));

            var btn = Instantiate(upgradeItemButtonPrefab, recipeListContainer);
            btn.Init(item, recipe, this);
        }

        Debug.Log($"<color=#FFFF00>[UPGRADE] Final UI Count = {candidates.Count}</color>");
    }


    // ============================================================
    //  КЛИК по предмету в левом списке
    // ============================================================
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

    // ============================================================
    //  СОБЫТИЯ ПРОЦЕССОРА
    // ============================================================
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
