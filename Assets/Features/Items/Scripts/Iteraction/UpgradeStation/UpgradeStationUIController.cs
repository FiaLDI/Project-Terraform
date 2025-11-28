using UnityEngine;

public class UpgradeStationUIController : BaseStationUI
{
    private UpgradeStation station;
    private UpgradeProcessor processor;

    public void Init(UpgradeStation station, UpgradeProcessor processor)
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
            var slot = InventoryManager.instance.GetSelectedSlot();
            processor.BeginUpgrade(recipe, slot);
        });
    }

    private void HandleStart(Item item) => recipePanel.StartProgress();
    private void HandleProgress(float t) => recipePanel.UpdateProgress(t);
    private void HandleComplete(Item item) => recipePanel.ProcessComplete();
}

