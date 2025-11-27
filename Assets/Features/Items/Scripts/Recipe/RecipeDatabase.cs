using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Crafting/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [Header("All recipes in game")]
    public RecipeSO[] recipes;

    // --- INTERNAL CACHED LISTS ---
    private RecipeSO[] cachedWorkbench;
    private RecipeSO[] cachedProcessor;
    private RecipeSO[] cachedUpgrade;

    private Dictionary<string, RecipeSO> recipeById;


    // ============================
    // INITIALIZATION
    // ============================
    private void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        // Build dictionary by ID
        recipeById = new Dictionary<string, RecipeSO>();

        foreach (var r in recipes)
        {
            if (r == null || string.IsNullOrEmpty(r.recipeId))
                continue;

            if (!recipeById.ContainsKey(r.recipeId))
                recipeById.Add(r.recipeId, r);
            else
                Debug.LogWarning($"[RecipeDatabase] Duplicate recipeId: {r.recipeId}");
        }

        // Cached categories
        cachedWorkbench = Filter(r => r.requiresWorkbench);
        cachedProcessor = Filter(r => r.requiresProcessor);
        cachedUpgrade  = Filter(r => r.requiresUpgradeStation);
    }


    // ============================
    // FILTER HELPERS
    // ============================
    private RecipeSO[] Filter(System.Predicate<RecipeSO> predicate)
    {
        List<RecipeSO> result = new List<RecipeSO>();
        foreach (var r in recipes)
        {
            if (r != null && predicate(r))
                result.Add(r);
        }
        return result.ToArray();
    }


    // ============================
    // PUBLIC API
    // ============================

    /// <summary>
    /// Найти рецепт по ID
    /// </summary>
    public RecipeSO GetRecipeById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        recipeById.TryGetValue(id, out var r);
        return r;
    }

    /// <summary>
    /// Верстак — сборка предметов
    /// </summary>
    public RecipeSO[] GetForWorkbench()
        => cachedWorkbench;

    /// <summary>
    /// Переработчик материалов
    /// </summary>
    public RecipeSO[] GetForProcessor()
        => cachedProcessor;

    /// <summary>
    /// Станция улучшения предметов
    /// </summary>
    public RecipeSO[] GetForUpgrade()
        => cachedUpgrade;
}
