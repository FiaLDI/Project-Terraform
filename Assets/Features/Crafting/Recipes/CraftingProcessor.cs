using System;
using System.Collections;
using UnityEngine;
using Features.Items.Domain;
using Features.Inventory;
using Features.Inventory.Application;
using Features.Player;
using Features.Inventory.Domain;

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

        var player = LocalPlayerContext.Player;
        if (player == null)
            return;

        var net = player.GetComponent<InventoryStateNetwork>();
        if (net == null)
            return;

        net.RequestInventoryCommand(new InventoryCommandData
        {
            Command  = InventoryCommand.CraftRecipe,
            RecipeId = recipe.recipeId
        });

        OnComplete?.Invoke(recipe);
    }



    // ======================================================
    // OPTIONAL API
    // ======================================================

    public bool IsProcessing => isProcessing;
}
