using UnityEngine;

[CreateAssetMenu(menuName = "Items/Recipe/Crafting")]
public class CraftingRecipeSO : RecipeSO
{
    private void OnValidate()
    {
        recipeType = RecipeType.Crafting;
        stationType = StationType.Workbench;
    }
}
