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

    // --------------------------------------------------------------------
    //  PUBLIC API
    // --------------------------------------------------------------------
    public void BeginUpgrade(RecipeSO recipe, InventorySlot slot)
    {
        if (recipe == null || slot == null || slot.ItemData == null)
        {
            Debug.LogError("[UpgradeProcessor] Неверный рецепт или слот.");
            return;
        }

        Item item = slot.ItemData;

        // 1) Проверяем, что предмет поддерживает улучшение
        if (item.upgrades == null || item.upgrades.Length == 0)
        {
            Debug.LogWarning($"[UpgradeProcessor] Предмет {item.itemName} не имеет апгрейдов.");
            return;
        }

        // 2) Проверяем, что есть следующий уровень
        if (item.currentLevel >= item.upgrades.Length - 1)
        {
            Debug.LogWarning($"[UpgradeProcessor] {item.itemName} уже на максимальном уровне!");
            return;
        }

        // 3) Проверяем, что хватает ресурсов
        if (!InventoryManager.instance.HasIngredients(recipe.inputs))
        {
            Debug.LogWarning("[UpgradeProcessor] Недостаточно ресурсов для улучшения.");
            return;
        }

        // 4) Всё ОК — стартуем
        Debug.Log($"[UpgradeProcessor] Запуск улучшения предмета {item.itemName}, уровень {item.currentLevel + 1}");

        activeRecipe = recipe;
        targetSlot = slot;
        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(item);
    }

    // --------------------------------------------------------------------
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

    // --------------------------------------------------------------------
    private void CompleteUpgrade()
    {
        isProcessing = false;

        Item item = targetSlot.ItemData;

        Debug.Log($"[UpgradeProcessor] Улучшение завершено: {item.itemName}");

        // 1) Списываем ресурсы
        InventoryManager.instance.ConsumeIngredients(activeRecipe.inputs);

        // 2) Повышаем уровень предмета
        item.currentLevel++;

        var stats = ItemStatCalculator.Calculate(item);
        EquipmentManager.instance.ApplyRuntimeStats(item, stats);

        // 3) Если нужно — можно применить статы
        ApplyUpgradeStats(item);

        // 4) Сообщаем UI
        OnComplete?.Invoke(item);

        // 5) Обновление UI инвентаря
        InventoryManager.instance.UpdateAllUI();
    }

    // --------------------------------------------------------------------
    //  OPTIONAL: применение статов предмета (если нужно)
    // --------------------------------------------------------------------
    private void ApplyUpgradeStats(Item item)
    {
        var upgradeData = item.CurrentUpgrade;
        if (upgradeData == null)
            return;

        // Пример:
        // if (item is Weapon w)
        // {
        //     w.damage = upgradeData.damage;
        //     w.fireRate = upgradeData.fireRate;
        // }

        Debug.Log($"[UpgradeProcessor] Применены статусы апгрейда: {item.itemName} lvl {item.currentLevel}");
    }
}
