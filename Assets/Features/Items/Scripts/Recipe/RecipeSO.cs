using UnityEngine;

[CreateAssetMenu(menuName = "Items/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("ID")]
    public string recipeId;

    [Header("Input Ingredients")]
    public RecipeIngredient[] inputs;
    public RecipeIngredient[] ingredients => inputs;

    [Header("Output (для крафта)")]
    public Item outputItem;
    public int outputAmount = 1;

    [Header("Crafting / Upgrade Duration")]
    public float duration = 2f;

    [Header("Station Type")]
    public StationType stationType = StationType.None;

    [Header("Recipe Type")]
    public RecipeType recipeType = RecipeType.Crafting;

    [Header("Upgrade Settings")]
    [Tooltip("Предмет, который улучшается")]
    public Item upgradeBaseItem;
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

public enum StationType
{
    None,
    Workbench,
    Processor,
    UpgradeStation
}
