using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Button button;

    private RecipeSO recipe;
    private PlayerBoundStationUI ui;
    private BaseStationUI legacyUi;

    public void Init(RecipeSO recipe, PlayerBoundStationUI ui)
    {
        this.recipe = recipe;
        this.ui = ui;
        legacyUi = null;

        BindVisuals();
    }

    public void Init(RecipeSO recipe, BaseStationUI ui)
    {
        this.recipe = recipe;
        legacyUi = ui;
        this.ui = null;

        BindVisuals();
    }

    private void BindVisuals()
    {
        if (recipe == null)
            return;

        if (recipe.recipeType == RecipeType.Upgrade)
        {
            if (icon != null)
                icon.sprite = recipe.upgradeBaseItem.icon;

            if (title != null)
                title.text = $"{recipe.upgradeBaseItem.itemName}  (Upgrade)";
        }
        else
        {
            if (icon != null)
                icon.sprite = recipe.outputItem.icon;

            if (title != null)
                title.text = recipe.outputItem.itemName;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (ui != null)
        {
            ui.ShowRecipe(recipe);
            return;
        }

        legacyUi?.ShowRecipe(recipe);
    }
}
