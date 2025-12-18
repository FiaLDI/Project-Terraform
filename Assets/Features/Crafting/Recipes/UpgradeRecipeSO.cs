using UnityEngine;

[CreateAssetMenu(menuName = "Items/Recipe/Upgrade")]
public class UpgradeRecipeSO : RecipeSO
{
    public int targetLevel = 0; // можно не использовать, будет игнорироваться

    private void OnValidate()
    {
        recipeType = RecipeType.Upgrade;
        stationType = StationType.UpgradeStation;
    }
}
