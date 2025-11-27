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

    protected CraftingProcessor processor;
    protected bool isOpen = false;

    // --------------------------------------------------------------------

    protected virtual void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Toggle);

        canvas.enabled = false;
    }

    // --------------------------------------------------------------------
    // MAIN INITIALIZER (этого метода у тебя нет — из-за этого ошибка!)
    // --------------------------------------------------------------------

    public void Init(CraftingProcessor processor, RecipeSO[] recipes)
    {
        this.processor = processor;

        Populate(recipes);

        processor.OnStart += _ => recipePanel.StartProgress();
        processor.OnProgress += t => recipePanel.UpdateProgress(t);
        processor.OnComplete += r => recipePanel.OnProcessComplete();
    }

    // --------------------------------------------------------------------
    // POPULATE RECIPE BUTTONS
    // --------------------------------------------------------------------

    protected void Populate(RecipeSO[] recipes)
    {
        foreach (Transform t in recipeListContainer)
            Destroy(t.gameObject);

        foreach (var recipe in recipes)
        {
            Debug.Log($"Adding recipe: {recipe.recipeId}, output={recipe.outputItem}");
            var btn = Instantiate(recipeButtonPrefab, recipeListContainer);
            btn.Init(recipe, this);
        }
    }

    // --------------------------------------------------------------------
    // UI TOGGLE
    // --------------------------------------------------------------------

    public void Toggle()
    {
        isOpen = !isOpen;

        canvas.enabled = isOpen;
        PlayerUsageController.InteractionLocked = isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // --------------------------------------------------------------------
    // SHOW RECIPE DETAILS
    // --------------------------------------------------------------------

    public void ShowRecipe(RecipeSO recipe)
    {
        recipePanel.Show(recipe, processor);
    }
}
