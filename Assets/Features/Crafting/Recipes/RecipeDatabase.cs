using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Items/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [Header("All recipes in game")]
    public RecipeSO[] recipes;
    public static RecipeDatabase Instance;

    private Dictionary<string, RecipeSO> recipeById;
    private RecipeSO[] cachedWorkbench;
    private RecipeSO[] cachedProcessor;
    private RecipeSO[] cachedUpgrade;

    private void OnEnable()
    {
        Instance = this;
        BuildCache();
    }

    private void BuildCache()
    {
        recipeById = recipes
            .Where(r => r != null && !string.IsNullOrEmpty(r.recipeId))
            .ToDictionary(r => r.recipeId, r => r);

        cachedWorkbench = recipes
            .Where(r => r.stationType == StationType.Workbench)
            .ToArray();

        cachedProcessor = recipes
            .Where(r => r.stationType == StationType.Processor)
            .ToArray();

        cachedUpgrade = recipes
            .Where(r => r.stationType == StationType.UpgradeStation)
            .ToArray();
    }

    public RecipeSO GetRecipeById(string id)
    {
        if (recipeById.TryGetValue(id, out var r)) return r;
        return null;
    }

    public RecipeSO[] GetForWorkbench() => cachedWorkbench;
    public RecipeSO[] GetForProcessor() => cachedProcessor;
    public RecipeSO[] GetForUpgrade() => cachedUpgrade;
}
