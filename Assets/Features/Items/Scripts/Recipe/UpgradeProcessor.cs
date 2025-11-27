using System;
using UnityEngine;

public class UpgradeProcessor : MonoBehaviour
{
    public event Action<RecipeSO> OnStart;
    public event Action<float> OnProgress;
    public event Action<RecipeSO> OnComplete;

    private RecipeSO activeRecipe;
    private float startTime;
    private bool isProcessing;

    private InventorySlot targetSlot;

    // --------------------------------------------------------
    // PUBLIC ENTRY POINT
    // --------------------------------------------------------
    public void BeginUpgrade(RecipeSO recipe, InventorySlot slot)
    {
        if (recipe == null) return;
        if (slot == null) return;
        if (slot.ItemData == null) return;

        if (recipe.recipeType != RecipeType.Upgrade)
        {
            Debug.LogWarning("[UpgradeProcessor] Wrong recipe type!");
            return;
        }

        activeRecipe = recipe;
        targetSlot = slot;

        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(recipe);
    }

    // --------------------------------------------------------
    private void Update()
    {
        if (!isProcessing) return;

        float progress = (Time.time - startTime) / activeRecipe.duration;
        OnProgress?.Invoke(progress);

        if (progress >= 1f)
            FinishUpgrade();
    }

    // --------------------------------------------------------
    private void FinishUpgrade()
    {
        isProcessing = false;

        // 1. тратим ингредиенты
        InventoryManager.instance.ConsumeIngredients(activeRecipe.ingredients);

        // 2. апгрейд предмета
        UpgradeItem(targetSlot);

        // 3. уведомляем UI
        OnComplete?.Invoke(activeRecipe);
    }

    // --------------------------------------------------------
    // MAIN LOGIC: level++
    // --------------------------------------------------------
    private void UpgradeItem(InventorySlot slot)
    {
        Item item = slot.ItemData;

        if (item.upgrades == null || item.upgrades.Length == 0)
        {
            Debug.LogWarning($"[UpgradeProcessor] {item.itemName} has no upgrade data!");
            return;
        }

        if (item.currentLevel >= item.upgrades.Length - 1)
        {
            Debug.LogWarning($"[UpgradeProcessor] {item.itemName} is already max level!");
            return;
        }

        item.currentLevel++;

        Debug.Log($"[UpgradeProcessor] {item.itemName} upgraded to level {item.currentLevel}");

        slot.NotifyChanged();
    }
}
