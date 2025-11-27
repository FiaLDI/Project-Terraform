using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipePanelUI : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private Button actionButton;

    [Header("Progress UI")]
    [SerializeField] private CraftingProgressUI progressUI;

    public void ShowRecipe(RecipeSO recipe)
    {
        gameObject.SetActive(true);

        icon.sprite = recipe.outputItem.icon;
        title.text = recipe.outputItem.itemName;

        ingredientsText.text = "";
        foreach (var ing in recipe.inputs)
            ingredientsText.text += $"{ing.item.itemName} x {ing.amount}\n";

        progressUI.SetVisible(false);
        progressUI.UpdateProgress(0f);

        actionButton.onClick.RemoveAllListeners();
    }

    public void SetAction(System.Action callback)
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => callback?.Invoke());
    }

    public void StartProgress()
    {
        progressUI.SetVisible(true);
        progressUI.UpdateProgress(0f);
    }

    public void UpdateProgress(float t)
    {
        progressUI.UpdateProgress(t);
    }

    public void ProcessComplete()
    {
        progressUI.UpdateProgress(1f);
        progressUI.PlayCompleteAnimation();
    }

    public void ShowMissingIngredients()
    {
        ingredientsText.text += "\n<color=#ff4444>Недостаточно ресурсов!</color>";
    }


    public void ShowError(string msg)
    {
        Debug.LogWarning("[RecipePanelUI] " + msg);
    }
}
