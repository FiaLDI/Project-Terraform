using Features.Input;
using Features.UI;
using UnityEngine;

public abstract class PlayerBoundStationUI : PlayerBoundUIView, IUIScreen
{
    [SerializeField] private GameObject screenRoot;

    public InputMode Mode => InputMode.Inventory;

    protected bool IsVisible => screenRoot != null && screenRoot.activeSelf;

    protected virtual void Awake()
    {
        if (screenRoot == null)
        {
            Debug.LogError($"[{name}] Screen root is not assigned", this);
            enabled = false;
            return;
        }

        if (screenRoot == gameObject)
        {
            Debug.LogError($"[{name}] Screen root must be a child object, not the component root", this);
            enabled = false;
            return;
        }

        screenRoot.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);

        if (screenRoot != null)
            screenRoot.SetActive(false);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);

        if (screenRoot != null)
            screenRoot.SetActive(false);

        base.OnDisable();
    }

    public virtual void Show()
    {
        if (screenRoot != null)
            screenRoot.SetActive(true);

        InputModeManager.I?.SetMode(Mode);
    }

    public virtual void Hide()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);

        InputModeManager.I?.SetMode(InputMode.Gameplay);
    }

    public virtual void Open()
    {
        UIStackManager.I?.Push(this);
    }

    public virtual void Close()
    {
        UIStackManager.I?.Pop();
    }

    public abstract void ShowRecipe(RecipeSO recipe);

    protected void PopulateRecipeList(RecipeSO[] recipes, Transform recipeListContainer, RecipeButtonUI recipeButtonPrefab)
    {
        if (recipeListContainer == null || recipeButtonPrefab == null)
            return;

        foreach (Transform child in recipeListContainer)
            Destroy(child.gameObject);

        if (recipes == null)
            return;

        foreach (RecipeSO recipe in recipes)
        {
            if (recipe == null)
                continue;

            RecipeButtonUI button = Instantiate(recipeButtonPrefab, recipeListContainer);
            button.Init(recipe, this);
        }
    }
}
