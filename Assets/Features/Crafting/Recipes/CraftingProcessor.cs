using System;
using System.Collections;
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

    private bool isProcessing;
    private Coroutine currentRoutine;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(IInventoryContext inventory)
    {
        this.inventory = inventory;
        Debug.Log("[Crafting] Init called");
    }

    // ======================================================
    // PUBLIC API
    // ======================================================

    public void Begin(RecipeSO recipe)
    {
        if (isProcessing)
        {
            Debug.LogWarning("[Crafting] Already processing");
            return;
        }

        if (recipe == null)
        {
            Debug.LogError("[Crafting] Recipe is null");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("[Crafting] Inventory is null");
            return;
        }

        Debug.Log($"[Crafting] Start recipe '{recipe.name}', duration = {recipe.duration}");

        currentRoutine = StartCoroutine(Process(recipe));
    }

    // ======================================================
    // PROCESS
    // ======================================================

    private IEnumerator Process(RecipeSO recipe)
    {
        isProcessing = true;

        OnStart?.Invoke(recipe);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, recipe.duration); // защита от 0

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            OnProgress?.Invoke(progress);

            yield return null;
        }

        FinishRecipe(recipe);
    }

    // ======================================================
    // COMPLETE
    // ======================================================

    private void FinishRecipe(RecipeSO recipe)
    {
        isProcessing = false;

        var service = inventory.Service;

        // 1️⃣ REMOVE INGREDIENTS
        foreach (var ing in recipe.ingredients)
        {
            bool removed = service.TryRemove(ing.item, ing.amount);
            if (!removed)
            {
                Debug.LogError($"[Crafting] Failed to remove ingredient: {ing.item.name}");
                return;
            }
        }

        // 2️⃣ ADD OUTPUT
        var output = recipe.outputItem;
        if (output == null)
        {
            Debug.LogWarning("[Crafting] Recipe has no output item");
            return;
        }

        if (output.isStackable)
        {
            service.AddItem(new ItemInstance(output, recipe.outputAmount));
        }
        else
        {
            for (int i = 0; i < recipe.outputAmount; i++)
                service.AddItem(new ItemInstance(output, 1));
        }

        OnComplete?.Invoke(recipe);
    }

    // ======================================================
    // OPTIONAL API
    // ======================================================

    public bool IsProcessing => isProcessing;
}
