using UnityEngine;

public class WorkbenchUIController : BaseStationUI
{
    private Workbench station;
    private CraftingProcessor processor;

    public void Init(Workbench station, CraftingProcessor processor)
    {
        this.station = station;
        this.processor = processor;

        base.Init(station.GetRecipes());

        processor.OnStart += HandleStart;
        processor.OnProgress += HandleProgress;
        processor.OnComplete += HandleComplete;
    }

    public override void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);

        recipePanel.SetAction(() =>
        {
            if (!InventoryManager.instance.HasIngredients(recipe.inputs))
            {
                recipePanel.ShowMissingIngredients();
                return;
            }

            processor.Begin(recipe);
        });
    }


    private void HandleStart(RecipeSO r) => recipePanel.StartProgress();
    private void HandleProgress(float t) => recipePanel.UpdateProgress(t);
    private void HandleComplete(RecipeSO r) => recipePanel.ProcessComplete();
}
