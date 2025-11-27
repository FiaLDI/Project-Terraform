using System;
using UnityEngine;

public class CraftingProcessor : MonoBehaviour
{
    public event Action<RecipeSO> OnStart;
    public event Action<float> OnProgress;
    public event Action<RecipeSO> OnComplete;

    private RecipeSO activeRecipe;
    private float startTime;
    private bool isProcessing;

    public void StartRecipe(RecipeSO recipe)
    {
        if (recipe == null) return;

        activeRecipe = recipe;
        startTime = Time.time;
        isProcessing = true;

        OnStart?.Invoke(recipe);
    }

    private void Update()
    {
        if (!isProcessing) return;

        float progress = (Time.time - startTime) / activeRecipe.duration;
        OnProgress?.Invoke(progress);

        if (progress >= 1f)
        {
            FinishRecipe();
        }
    }

    private void FinishRecipe()
    {
        isProcessing = false;

        InventoryManager.instance.AddItem(activeRecipe.outputItem, activeRecipe.outputAmount);

        InventoryManager.instance.ConsumeIngredients(activeRecipe.ingredients);

        OnComplete?.Invoke(activeRecipe);
    }

    public void Begin(RecipeSO recipe)
    {
        StartRecipe(recipe);
    }

}
