using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Button button;

    private RecipeSO recipe;
    private BaseStationUI ui;

    public void Init(RecipeSO recipe, BaseStationUI ui)
    {
        this.recipe = recipe;
        this.ui = ui;

        if (recipe.outputItem != null)
        {
            if (icon != null)
                icon.sprite = recipe.outputItem.icon;

            if (title != null)
                title.text = recipe.outputItem.itemName;
        }
        else if (recipe.upgradeBaseItem != null)
        {
            if (icon != null)
                icon.sprite = recipe.upgradeBaseItem.icon;

            if (title != null)
                title.text = $"{recipe.upgradeBaseItem.itemName} (Upgrade)";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ui.ShowRecipe(recipe));
    }
}
