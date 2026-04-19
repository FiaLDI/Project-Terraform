using Features.Inventory;
using Features.Player;
using UnityEngine;
using UnityEngine.UI;

public abstract class CraftingRecipeStationUIBase : PlayerBoundStationUI
{
    private IInventoryContext inventory;
    private bool initialized;

    protected IInventoryContext Inventory => inventory;

    protected abstract CraftingProcessor Processor { get; }
    protected abstract Button CloseButton { get; }
    protected abstract Transform RecipeListContainer { get; }
    protected abstract RecipeButtonUI RecipeButtonPrefab { get; }
    protected abstract RecipePanelUI RecipePanel { get; }

    protected sealed override void OnPlayerBound(GameObject player)
    {
        if (initialized)
            return;

        if (Processor == null || RecipePanel == null)
        {
            Debug.LogError($"[{GetType().Name}] Missing required references", this);
            return;
        }

        inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
        {
            Debug.LogError($"[{GetType().Name}] Local inventory not available", this);
            return;
        }

        Processor.Init(inventory);
        RecipePanel.Init(inventory);
        PopulateRecipes();

        Processor.OnStart += HandleStart;
        Processor.OnProgress += HandleProgress;
        Processor.OnComplete += HandleComplete;

        if (CloseButton != null)
        {
            CloseButton.onClick.RemoveListener(Close);
            CloseButton.onClick.AddListener(Close);
        }

        OnInitialized();
        initialized = true;
    }

    protected virtual void OnDestroy()
    {
        if (Processor == null)
            return;

        Processor.OnStart -= HandleStart;
        Processor.OnProgress -= HandleProgress;
        Processor.OnComplete -= HandleComplete;
    }

    public override void ShowRecipe(RecipeSO recipe)
    {
        RecipePanel.ShowRecipe(recipe);
        RecipePanel.SetAction(() =>
        {
            if (!HasIngredients(recipe))
            {
                RecipePanel.ShowMissingIngredients(recipe);
                return;
            }

            Processor.Begin(recipe);
        });
        RecipePanel.SetCancelAction(CancelProcessing);
    }

    public override void Hide()
    {
        CancelProcessing();
        base.Hide();
    }

    public override void Close()
    {
        CancelProcessing();
        base.Close();
    }

    protected virtual void OnInitialized()
    {
    }

    protected abstract RecipeSO[] GetRecipes(RecipeDatabase recipeDatabase);

    private void PopulateRecipes()
    {
        RecipeDatabase recipeDatabase = RecipeDatabase.Instance;

        RecipeSO[] recipes = GetRecipes(recipeDatabase);
        if (recipes == null)
        {
            Debug.LogError($"[{GetType().Name}] Recipes are not available", this);
            return;
        }

        PopulateRecipeList(recipes, RecipeListContainer, RecipeButtonPrefab);
    }

    private bool HasIngredients(RecipeSO recipe)
    {
        return inventory != null &&
            inventory.Service != null &&
            inventory.Service.HasIngredients(recipe.inputs);
    }

    private void HandleStart(RecipeSO recipe)
        => RecipePanel.StartProgress();

    private void HandleProgress(float progress)
        => RecipePanel.UpdateProgress(progress);

    private void HandleComplete(RecipeSO recipe)
    {
        RecipePanel.ProcessComplete();
        RecipePanel.RefreshIngredients();
    }

    private void CancelProcessing()
    {
        if (Processor != null && Processor.IsProcessing)
            Processor.Cancel();

        if (RecipePanel != null)
            RecipePanel.ResetProgress();
    }
}
