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

        icon.sprite = recipe.outputItem.icon;
        title.text = recipe.outputItem.itemName;

        button.onClick.AddListener(() => ui.ShowRecipe(recipe));
    }
}
