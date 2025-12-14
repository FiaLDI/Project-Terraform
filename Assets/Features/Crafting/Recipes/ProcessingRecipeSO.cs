using UnityEngine;

[CreateAssetMenu(menuName = "Items/Recipe/Processing")]
public class ProcessingRecipeSO : RecipeSO
{
    private void OnValidate()
    {
        recipeType = RecipeType.Processing;
        stationType = StationType.Processor;
    }
}
