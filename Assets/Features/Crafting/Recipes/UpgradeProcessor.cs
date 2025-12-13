using System;
using System.Collections;
using UnityEngine;
using Features.Items.Domain;
using Features.Inventory;
using Features.Inventory.Application;

public class UpgradeProcessor : MonoBehaviour
{
    public event Action<ItemInstance> OnStart;
    public event Action<float> OnProgress;
    public event Action<ItemInstance> OnComplete;

    private IInventoryContext inventory;

    private bool isProcessing;
    private Coroutine currentRoutine;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(IInventoryContext inventory)
    {
        this.inventory = inventory;
    }

    // ======================================================
    // PUBLIC API
    // ======================================================

    public void BeginUpgrade(UpgradeRecipeSO recipe, ItemInstance inst)
    {
        if (isProcessing)
            return;

        if (recipe == null || inst == null || inst.itemDefinition == null)
            return;

        if (inventory == null)
            return;

        int maxLevels = inst.itemDefinition.upgrades?.Length ?? 0;
        if (inst.level >= maxLevels)
            return;

        if (!inventory.Service.HasIngredients(recipe.ingredients))
            return;

        currentRoutine = StartCoroutine(Process(recipe, inst));
    }

    // ======================================================
    // PROCESS
    // ======================================================

    private IEnumerator Process(UpgradeRecipeSO recipe, ItemInstance inst)
    {
        isProcessing = true;

        OnStart?.Invoke(inst);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, recipe.duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            OnProgress?.Invoke(elapsed / duration);
            yield return null;
        }

        Finish(recipe, inst);
    }

    // ======================================================
    // COMPLETE
    // ======================================================

    private void Finish(UpgradeRecipeSO recipe, ItemInstance inst)
    {
        isProcessing = false;
        currentRoutine = null;

        var service = inventory.Service;

        // 1️⃣ REMOVE INGREDIENTS
        foreach (var ing in recipe.ingredients)
            service.TryRemove(ing.item, ing.amount);

        // 2️⃣ APPLY UPGRADE
        inst.level++;

        // 🔥 КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ
        // Сообщаем всем UI, что модель изменилась
        service.NotifyChanged();

        OnComplete?.Invoke(inst);
    }

    // ======================================================
    // OPTIONAL
    // ======================================================

    public bool IsProcessing => isProcessing;

    public void Cancel()
    {
        if (!isProcessing)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = null;
        isProcessing = false;
    }
}
