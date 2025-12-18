using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Inventory;

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

    private IInventoryContext inventory;

    private RecipeSO currentRecipe;
    private ItemInstance currentInstance;
    private Action currentAction;

    // ========================================================
    // INIT
    // ========================================================

    public void Init(IInventoryContext inventory)
    {
        this.inventory = inventory;

        if (inventory != null)
            inventory.Service.OnChanged += RefreshIngredients;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Service.OnChanged -= RefreshIngredients;
    }

    // ========================================================
    // ACTION BUTTON
    // ========================================================

    public void SetAction(Action action)
    {
        currentAction = action;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => currentAction?.Invoke());
    }

    // ========================================================
    // CRAFT VIEW
    // ========================================================

    public void ShowRecipe(RecipeSO recipe)
    {
        currentRecipe = recipe;
        currentInstance = null;

        gameObject.SetActive(true);

        if (icon != null && recipe.outputItem != null)
            icon.sprite = recipe.outputItem.icon;

        if (title != null)
            title.text = recipe.outputItem.itemName;

        RefreshIngredients();

        progressUI.SetVisible(false);
        actionButton.onClick.RemoveAllListeners();
    }

    // ========================================================
    // UPGRADE VIEW
    // ========================================================

    public void ShowUpgradeRecipe(ItemInstance inst, RecipeSO recipe)
    {
        currentRecipe = recipe;
        currentInstance = inst;

        gameObject.SetActive(true);

        var def = inst.itemDefinition;
        var next = def.upgrades[inst.level];

        icon.sprite = def.icon;
        title.text = def.itemName + " — Upgrade";

        upgradeInfoText.text =
            $"Current: Lv {inst.level}\n" +
            $"Next: Lv {inst.level + 1}\n" +
            next.ToStatsText();

        upgradePreviewIcon.sprite =
            next.UpgradedIcon != null ? next.UpgradedIcon : def.icon;

        RefreshIngredients();
        progressUI.SetVisible(false);
    }

    // ========================================================
    // INGREDIENTS
    // ========================================================

    public void RefreshIngredients()
    {
        if (currentRecipe == null || ingredientsText == null || inventory == null)
            return;

        ingredientsText.text = "";

        foreach (var ing in currentRecipe.ingredients)
        {
            int have = inventory.Service.GetItemCount(ing.item);
            bool enough = have >= ing.amount;
            string color = enough ? "#FFFFFF" : "#FF4444";

            ingredientsText.text +=
                $"<color={color}>{ing.item.itemName}: {have}/{ing.amount}</color>\n";
        }
    }

    // ========================================================
    // PROGRESS
    // ========================================================

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
        Invoke(nameof(HideProgress), 0.2f);
    }

    private void HideProgress()
    {
        progressUI.SetVisible(false);
    }

    // ========================================================
    // CONTROL
    // ========================================================

    public void Close()
    {
        ResetProgress();
        gameObject.SetActive(false);
    }

    public void ResetProgress()
    {
        progressUI.SetVisible(false);
        progressUI.UpdateProgress(0f);
    }

    public void Clear()
    {
        currentRecipe = null;
        currentInstance = null;
        currentAction = null;

        Close();
    }

    public void RefreshUpgradeInfo()
    {
        if (currentInstance == null || currentRecipe == null)
            return;

        var def = currentInstance.itemDefinition;
        if (def == null || def.upgrades == null)
            return;

        if (currentInstance.level >= def.upgrades.Length)
        {
            Clear();
            return;
        }

        var next = def.upgrades[currentInstance.level];

        upgradeInfoText.text =
            $"Current: Lv {currentInstance.level}\n" +
            $"Next: Lv {currentInstance.level + 1}\n" +
            next.ToStatsText();

        upgradePreviewIcon.sprite =
            next.UpgradedIcon != null ? next.UpgradedIcon : def.icon;
    }

    public void ShowMissingIngredients(RecipeSO recipe)
    {
        currentRecipe = recipe;

        RefreshIngredients();

        Debug.Log($"[RecipePanelUI] Not enough ingredients for recipe: {recipe.name}");
    }

}
