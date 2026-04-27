using UnityEngine;
using UnityEngine.UI;

public sealed class WorkbenchUI : CraftingRecipeStationUIBase
{
    [SerializeField] private CraftingProcessor processor;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private RecipeButtonUI recipeButtonPrefab;
    [SerializeField] private RecipePanelUI recipePanel;

    protected override CraftingProcessor Processor => processor;
    protected override Button CloseButton => closeButton;
    protected override Transform RecipeListContainer => recipeListContainer;
    protected override RecipeButtonUI RecipeButtonPrefab => recipeButtonPrefab;
    protected override RecipePanelUI RecipePanel => recipePanel;

    protected override RecipeSO[] GetRecipes(RecipeDatabase recipeDatabase)
    {
        return recipeDatabase != null
            ? recipeDatabase.GetForWorkbench()
            : null;
    }
}
