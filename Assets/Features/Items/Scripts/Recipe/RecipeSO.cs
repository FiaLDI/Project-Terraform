using UnityEngine;

[CreateAssetMenu(menuName = "Items/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("ID")]
    public string recipeId;

    [Header("Input Ingredients")]
    public RecipeIngredient[] inputs;
    public RecipeIngredient[] ingredients => inputs;

    [Header("Output")]
    public Item outputItem;
    public int outputAmount = 1;

    [Header("Crafting Time")]
    public float duration = 2f;

    [Header("Station Requirements")]
    public bool requiresWorkbench = false;
    public bool requiresProcessor = false;
    public bool requiresUpgradeStation = false;

    [Header("Recipe Type")]
    public RecipeType recipeType = RecipeType.Crafting;
}

[System.Serializable]
public class RecipeIngredient
{
    public Item item;
    public int amount;
}

public enum RecipeType
{
    Processing,
    Crafting,
    Upgrade
}
