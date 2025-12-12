using System;
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
    private float startTime;

    private UpgradeRecipeSO activeRecipe;
    private ItemInstance targetInstance;

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
        if (recipe == null || inst == null || inventory == null)
        {
            Debug.LogError("[UpgradeProcessor] Invalid input or inventory");
            return;
        }

        int maxLevels = inst.itemDefinition.upgrades?.Length ?? 0;
        if (inst.level >= maxLevels)
        {
            Debug.Log("[Upgrade] Already max level");
            return;
        }

        if (!HasIngredients(recipe))
        {
            Debug.LogWarning("[Upgrade] Not enough ingredients");
            return;
        }

        activeRecipe = recipe;
        targetInstance = inst;

        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(inst);
    }

    // ======================================================
    // UPDATE
    // ======================================================

    private void Update()
    {
        if (!isProcessing || activeRecipe == null)
            return;

        float progress = (Time.time - startTime) / activeRecipe.duration;
        OnProgress?.Invoke(progress);

        if (progress >= 1f)
            Complete();
    }

    // ======================================================
    // INTERNAL
    // ======================================================

    private bool HasIngredients(RecipeSO recipe)
    {
        var service = inventory.Service;

        foreach (var ing in recipe.ingredients)
        {
            if (service.GetItemCount(ing.item) < ing.amount)
                return false;
        }

        return true;
    }

    private void Complete()
    {
        isProcessing = false;

        var service = inventory.Service;

        // REMOVE INGREDIENTS
        foreach (var ing in activeRecipe.ingredients)
            service.TryRemove(ing.item, ing.amount);

        // APPLY UPGRADE
        targetInstance.level++;

        OnComplete?.Invoke(targetInstance);
    }
}
