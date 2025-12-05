using UnityEngine;
using UnityEngine.UI;

public abstract class BaseStationUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Button closeButton;

    [Header("Recipe UI")]
    [SerializeField] protected Transform recipeListContainer;
    [SerializeField] protected RecipeButtonUI recipeButtonPrefab;
    [SerializeField] protected RecipePanelUI recipePanel;

    protected bool isOpen = false;
    public bool IsOpen => isOpen;


    protected virtual void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Toggle);

        canvas.enabled = false;
    }

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

    public void Toggle()
    {
        isOpen = !isOpen;

        canvas.enabled = isOpen;
        PlayerUsageController.InteractionLocked = isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        if (isOpen)
            UIStationManager.I.OpenStation(this);
        else
            UIStationManager.I.CloseStation(this);
    }

    public virtual void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.ShowRecipe(recipe);
    }

}
