using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipePanelUI : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI ingredientsText;
    [SerializeField] private Button craftButton;

    [Header("Progress UI (HP/Energy style)")]
    [SerializeField] private CraftingProgressUI progressUI;

    private RecipeSO recipe;
    private CraftingProcessor processor;

    public void Show(RecipeSO recipe, CraftingProcessor processor)
    {
        this.recipe = recipe;
        this.processor = processor;

        if (icon != null)
            icon.sprite = recipe.outputItem.icon;

        if (title != null)
            title.text = recipe.outputItem.itemName;

        if (ingredientsText != null)
        {
            ingredientsText.text = "";
            foreach (var ing in recipe.inputs)
                ingredientsText.text += $"{ing.item.itemName} x {ing.amount}\n";
        }

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                processor.Begin(recipe);
            });
        }

        if (progressUI != null)
        {
            progressUI.SetVisible(false);   // пока не крафтим — скрыт или 0%
            progressUI.UpdateProgress(0f);
        }

        gameObject.SetActive(true);
    }

    // Вызывается из BaseStationUI.OnStart
    public void StartProgress()
    {
        if (progressUI != null)
        {
            progressUI.SetVisible(true);
            progressUI.UpdateProgress(0f);
        }
    }

    // Вызывается из BaseStationUI.OnProgress
    public void UpdateProgress(float t)
    {
        if (progressUI != null)
            progressUI.UpdateProgress(t);
    }

    // Вызывается из BaseStationUI.OnComplete
    public void OnProcessComplete()
    {
        if (progressUI != null)
        {
            progressUI.UpdateProgress(1f);
            progressUI.PlayCompleteAnimation();
        }
    }
}
