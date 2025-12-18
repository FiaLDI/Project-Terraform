using UnityEngine;
using Features.Inventory;
using Features.Inventory.Application;

public class WorkbenchUIController : BaseStationUI
{
    private Workbench station;
    private CraftingProcessor processor;
    private IInventoryContext inventory;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(
        Workbench station,
        CraftingProcessor processor,
        IInventoryContext inventory)
    {
        this.station = station;
        this.processor = processor;
        this.inventory = inventory;

        recipePanel.Init(inventory);

        base.Init(station.GetRecipes());

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
    }

    // ======================================================
    // UI
    // ======================================================

    public override void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);

        recipePanel.SetAction(() =>
        {
            if (!inventory.Service.HasIngredients(recipe.inputs))
            {
                recipePanel.ShowMissingIngredients(recipe);
                return;
            }

            processor.Begin(recipe);
        });
    }

    // ======================================================
    // PROCESSOR EVENTS
    // ======================================================

    private void HandleStart(RecipeSO r)
        => recipePanel.StartProgress();

    private void HandleProgress(float t)
        => recipePanel.UpdateProgress(t);

    private void HandleComplete(RecipeSO r)
    {
        recipePanel.ProcessComplete();
        recipePanel.RefreshIngredients();
    }
}
