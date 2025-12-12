using System;
using UnityEngine;
using Features.Items.Domain;
using Features.Inventory;
using Features.Inventory.Application;

public class CraftingProcessor : MonoBehaviour
{
    public event Action<RecipeSO> OnStart;
    public event Action<float> OnProgress;
    public event Action<RecipeSO> OnComplete;

    private IInventoryContext inventory;

    private RecipeSO activeRecipe;
    private float startTime;
    private bool isProcessing;

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

    public void Begin(RecipeSO recipe)
    {
        if (recipe == null || inventory == null)
            return;

        activeRecipe = recipe;
        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(recipe);
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
            FinishRecipe();
    }

    // ======================================================
    // COMPLETE
    // ======================================================

    private void FinishRecipe()
    {
        isProcessing = false;

        var outputSO = activeRecipe.outputItem;
        if (outputSO == null)
        {
            Debug.LogWarning("[Crafting] Recipe has no output item");
            return;
        }

        var service = inventory.Service;

        // ADD OUTPUT
        if (outputSO.isStackable)
        {
            service.AddItem(new ItemInstance(outputSO, activeRecipe.outputAmount));
        }
        else
        {
            for (int i = 0; i < activeRecipe.outputAmount; i++)
                service.AddItem(new ItemInstance(outputSO, 1));
        }

        // REMOVE INGREDIENTS
        foreach (var ing in activeRecipe.ingredients)
            service.TryRemove(ing.item, ing.amount);

        OnComplete?.Invoke(activeRecipe);
    }
}
