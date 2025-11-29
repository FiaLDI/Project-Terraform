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
        if (!isProcessing || activeRecipe == null) return;

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

        Item template = activeRecipe.outputItem;

        if (template == null)
        {
            Debug.LogWarning("[CraftingProcessor] Recipe has no outputItem.");
            return;
        }

        // Нестакаемые → клонируем в рантайме
        if (!template.isStackable)
        {
            for (int i = 0; i < activeRecipe.outputAmount; i++)
            {
                Item runtimeItem = ItemFactory.CreateRuntimeItem(template);
                InventoryManager.instance.AddItem(runtimeItem, 1);
            }
        }
        else
        {
            // Стакуем по обычной схеме
            InventoryManager.instance.AddItem(template, activeRecipe.outputAmount);
        }

        InventoryManager.instance.ConsumeIngredients(activeRecipe.ingredients);

        OnComplete?.Invoke(activeRecipe);
    }

    public void Begin(RecipeSO recipe)
    {
        Debug.Log($"[PROCESSOR] Begin called for recipe {recipe.recipeId}");
        StartRecipe(recipe);
    }
}
