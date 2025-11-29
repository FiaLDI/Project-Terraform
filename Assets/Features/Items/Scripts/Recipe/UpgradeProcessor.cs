using System;
using UnityEngine;

public class UpgradeProcessor : MonoBehaviour
{
    public event Action<Item> OnStart;
    public event Action<float> OnProgress;
    public event Action<Item> OnComplete;

    private bool isProcessing = false;
    private float startTime;
    private RecipeSO activeRecipe;
    private InventorySlot targetSlot;

    public void BeginUpgrade(RecipeSO recipe, InventorySlot slot)
    {
        if (recipe == null || slot == null || slot.ItemData == null)
        {
            Debug.LogError("[UpgradeProcessor] Неверный рецепт или слот.");
            return;
        }

        Item item = slot.ItemData;

        if (item.upgrades == null || item.upgrades.Length == 0)
        {
            Debug.LogWarning($"[UpgradeProcessor] Предмет {item.itemName} не имеет апгрейдов.");
            return;
        }

        // Авто-проверка максимального уровня
        if (item.NextUpgrade == null)
        {
            Debug.LogWarning($"[UpgradeProcessor] {item.itemName} уже максимального уровня ({item.currentLevel}).");
            return;
        }

        if (!InventoryManager.instance.HasIngredients(recipe.inputs))
        {
            Debug.LogWarning("[UpgradeProcessor] Недостаточно ресурсов для улучшения.");
            return;
        }

        activeRecipe = recipe;
        targetSlot = slot;
        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(item);
    }

    private void Update()
    {
        if (!isProcessing || activeRecipe == null)
            return;

        float progress = (Time.time - startTime) / activeRecipe.duration;
        OnProgress?.Invoke(progress);

        if (progress >= 1f)
        {
            CompleteUpgrade();
        }
    }

    private void CompleteUpgrade()
    {
        isProcessing = false;

        Item item = targetSlot.ItemData;

        Debug.Log($"[UpgradeProcessor] Улучшение завершено: {item.itemName}");

        InventoryManager.instance.ConsumeIngredients(activeRecipe.inputs);

        item.currentLevel++;

        OnComplete?.Invoke(item);

        InventoryManager.instance.UpdateAllUI();
    }
}
