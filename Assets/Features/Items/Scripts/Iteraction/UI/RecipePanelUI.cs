using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [SerializeField] private InputActionReference cancelAction;

    [SerializeField] private BaseStationUI parentStation;

    private RecipeSO currentRecipe;
    private Item currentItem;

    private void Awake()
    {
        parentStation = GetComponentInParent<BaseStationUI>();
    }

    private void OnEnable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.performed += OnCancel;
            cancelAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.performed -= OnCancel;
            cancelAction.action.Disable();
        }
    }

    // -----------------------------
    //  Обычный крафт
    // -----------------------------
    public void ShowRecipe(RecipeSO recipe)
    {
        currentRecipe = recipe;
        currentItem = null;

        if (recipe.recipeType == RecipeType.Upgrade)
        {
            // Для Upgrade станция сама вызывает ShowUpgradeRecipe
            var slot = InventoryManager.Instance.GetSelectedSlot();
            if (slot != null && slot.ItemData != null)
            {
                ShowUpgradeRecipe(slot.ItemData, recipe);
                return;
            }

            if (title) title.text = "Select item to upgrade";
            if (ingredientsText) ingredientsText.text = "";
            return;
        }

        enabled = true;
        gameObject.SetActive(true);

        if (icon && recipe.outputItem != null) icon.sprite = recipe.outputItem.icon;
        if (title && recipe.outputItem != null) title.text = recipe.outputItem.itemName;

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

    // -----------------------------
    //  Upgrade режим
    // -----------------------------
    public void ShowUpgradeRecipe(Item item, RecipeSO recipe)
    {
        currentRecipe = recipe;
        currentItem = item;

        gameObject.SetActive(true);

        if (icon != null)
            icon.sprite = item.icon;

        if (title != null)
            title.text = $"{item.itemName} — Upgrade";

        int currentLevel = item.currentLevel;
        int maxLevel = (item.upgrades != null) ? item.upgrades.Length : 0;

        var next = item.NextUpgrade;

        if (next != null)
        {
            string statsText = "";
            if (next.bonusStats != null)
            {
                foreach (var stat in next.bonusStats)
                    statsText += $"{stat.stat}: +{stat.value}\n";
            }

            if (upgradeInfoText != null)
            {
                upgradeInfoText.text =
                    $"Current Level: {currentLevel}\n" +
                    $"Next Level: {currentLevel + 1}\n\n" +
                    statsText;
            }

            if (upgradePreviewIcon != null)
            {
                Sprite preview = next.UpgradedIcon != null ? next.UpgradedIcon : item.icon;
                upgradePreviewIcon.sprite = preview;
            }
        }
        else
        {
            if (upgradeInfoText != null)
                upgradeInfoText.text = $"MAX LEVEL ({currentLevel}/{maxLevel})";

            if (upgradePreviewIcon != null)
                upgradePreviewIcon.sprite = item.icon;
        }

        if (ingredientsText != null)
        {
            ingredientsText.text = "";
            foreach (var ing in recipe.inputs)
                ingredientsText.text += $"{ing.item.itemName} x {ing.amount}\n";
        }

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
    }

    public void ShowMissingIngredients(RecipeSO recipe)
    {
        if (ingredientsText == null) return;

        ingredientsText.text = "";

        foreach (var ing in recipe.inputs)
        {
            bool hasEnough = InventoryManager.Instance.HasItemCount(ing.item, ing.amount);
            string color = hasEnough ? "#FFFFFF" : "#FF4444";
            ingredientsText.text += $"<color={color}>{ing.item.itemName} x {ing.amount}</color>\n";
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (gameObject.activeSelf)
        {
            ClosePanel();
        }
        else
        {
            parentStation.Toggle();
        }
    }

    public void ClosePanel()
    {
        currentRecipe = null;
        currentItem = null;

        progressUI.SetVisible(false);
        actionButton.onClick.RemoveAllListeners();

        gameObject.SetActive(false);
    }
}
