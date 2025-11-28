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

    [Header("Crafting Time / Upgrade Duration")]
    public float duration = 2f;

    [Header("Station Requirements")]
    public bool requiresWorkbench = false;
    public bool requiresProcessor = false;
    public bool requiresUpgradeStation = false;

    [Header("Recipe Type")]
    public RecipeType recipeType = RecipeType.Crafting;

    [Header("Upgrade Settings")]
    [Tooltip("Какой предмет улучшает этот рецепт")]
    public Item upgradeBaseItem;

    [Tooltip("На какой уровень переводит этот рецепт (например 1, 2, 3...)")]
    public int upgradeTargetLevel = 1;
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
