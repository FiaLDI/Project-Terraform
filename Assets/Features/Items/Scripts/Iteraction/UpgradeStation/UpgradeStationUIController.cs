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

            if (slot == null || slot.ItemData == null)
            {
                recipePanel.ShowError("Выберите предмет!");
                return;
            }

            // ✔ Проверяем, тот ли предмет апгрейдим
            if (recipe.upgradeBaseItem != null &&
                slot.ItemData.id != recipe.upgradeBaseItem.id)
            {
                recipePanel.ShowError("Этот предмет нельзя улучшить по этому рецепту!");
                return;
            }

            // ✔ Проверяем наличие ресурсов
            if (!InventoryManager.instance.HasIngredients(recipe.inputs))
            {
                recipePanel.ShowError("Недостаточно ресурсов!");
                return;
            }

            processor.BeginUpgrade(recipe, slot);
        });
    }



    private void HandleStart(RecipeSO recipe)
    {
        recipePanel.StartProgress();
    }

    private void HandleProgress(float t)
    {
        recipePanel.UpdateProgress(t);
    }

    private void HandleComplete(RecipeSO recipe)
    {
        recipePanel.ProcessComplete();
    }
}
