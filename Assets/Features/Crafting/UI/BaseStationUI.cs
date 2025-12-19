using UnityEngine;
using UnityEngine.UI;
using Features.Input;

public abstract class BaseStationUI : MonoBehaviour, IUIScreen
{
    [Header("UI Root")]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Button closeButton;

    [Header("Recipe UI")]
    [SerializeField] protected Transform recipeListContainer;
    [SerializeField] protected RecipeButtonUI recipeButtonPrefab;
    [SerializeField] protected RecipePanelUI recipePanel;

    public InputMode Mode => InputMode.Inventory;

    protected virtual void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
        {
            Debug.LogError($"[{name}] Canvas not found", this);
            enabled = false;
            return;
        }

        canvas.enabled = false;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }


    // =========================
    // IUIScreen
    // =========================

    public virtual void Show()
    {
        canvas.enabled = true;
    }

    public virtual void Hide()
    {
        canvas.enabled = false;
    }

    // =========================
    // STACK API
    // =========================

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    public void Close()
    {
        UIStackManager.I.Pop();
    }

    // =========================
    // DATA
    // =========================

    public void Init(RecipeSO[] recipes)
    {
        Populate(recipes);
    }

    protected void Populate(RecipeSO[] recipes)
    {
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        foreach (var recipe in recipes)
        {
            var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
            btn.Init(recipe, this);
        }
    }

    public virtual void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);
    }
}
