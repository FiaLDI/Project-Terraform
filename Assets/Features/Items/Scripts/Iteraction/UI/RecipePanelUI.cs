using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipePanelUI : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI ingredientsText;

    [Header("Upgrade Info")]
    [SerializeField] private TextMeshProUGUI upgradeInfoText;
    [SerializeField] private Image upgradePreviewIcon;

    [Header("Action Button")]
    [SerializeField] private Button actionButton;

    [Header("Progress UI")]
    [SerializeField] private CraftingProgressUI progressUI;

    public void ShowRecipe(RecipeSO recipe)
    {
        gameObject.SetActive(true);

        if (recipe.recipeType == RecipeType.Upgrade)
        {
            ShowUpgradeRecipe(recipe);
            return;
        }

        // ===== обычный крафт =====
        if (icon) icon.sprite = recipe.outputItem.icon;
        if (title) title.text = recipe.outputItem.itemName;

        if (ingredientsText)
        {
            ingredientsText.text = "";
            foreach (var ing in recipe.inputs)
                ingredientsText.text += $"{ing.item.itemName} x {ing.amount}\n";
        }

        progressUI.SetVisible(false);
        progressUI.UpdateProgress(0f);
        actionButton.onClick.RemoveAllListeners();
    }

    private void ShowUpgradeRecipe(RecipeSO recipe)
    {
        InventorySlot slot = InventoryManager.instance.GetSelectedSlot();
        Item item = slot?.ItemData;

        title.text = $"{item.itemName} — Upgrade";

        // ===== показать апгрейд =====
        if (item.upgrades != null && item.currentLevel < item.upgrades.Length - 1)
        {
            ItemUpgradeData next = item.upgrades[item.currentLevel];

            // Сбор текста статов
            string statsText = "";
            foreach (var stat in next.bonusStats)
            {
                statsText += $"{stat.stat}: +{stat.value}\n";
            }

            upgradeInfoText.text =
                $"Current Level: {item.currentLevel}\n" +
                $"Next Level: {next.Level}\n\n" +
                statsText;

            if (next.Level > 0 && upgradePreviewIcon != null && next.Level < item.upgrades.Length)
            {
                if (next.Level < item.upgrades.Length)
                    upgradePreviewIcon.sprite = next.UpgradedIcon;
            }
        }
        else
        {
            upgradeInfoText.text = "MAX LEVEL";
        }

        // ===== показать ингредиенты =====
        ingredientsText.text = "";
        foreach (var ing in recipe.inputs)
            ingredientsText.text += $"{ing.item.itemName} x {ing.amount}\n";

        progressUI.SetVisible(false);
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

    public void UpdateProgress(float t) => progressUI.UpdateProgress(t);

    public void ProcessComplete()
    {
        progressUI.UpdateProgress(1f);
        progressUI.PlayCompleteAnimation();
    }

    public void ShowMissingIngredients(RecipeSO recipe)
    {
        ingredientsText.text = "";

        foreach (var ing in recipe.inputs)
        {
            bool hasEnough = InventoryManager.instance.HasItemCount(ing.item, ing.amount);
            string color = hasEnough ? "#FFFFFF" : "#FF4444";

            ingredientsText.text +=
                $"<color={color}>{ing.item.itemName} x {ing.amount}</color>\n";
        }
    }
}
