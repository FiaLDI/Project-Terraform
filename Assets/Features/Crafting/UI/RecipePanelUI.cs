using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
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

    [SerializeField] private InputActionReference cancelAction;

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
    }

    // ========================================================
    // LIFECYCLE
    // ========================================================

    private void OnEnable()
    {
        if (cancelAction != null)
        {
            cancelAction.action.Enable();
            cancelAction.action.performed += OnCancel;
        }

        if (inventory != null)
            inventory.Service.OnChanged += RefreshIngredients;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.Service.OnChanged -= RefreshIngredients;

        if (cancelAction != null)
        {
            cancelAction.action.performed -= OnCancel;
            cancelAction.action.Disable();
        }
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

        icon.sprite = def.icon;
        title.text = def.itemName + " — Upgrade";

        var next = def.upgrades[inst.level];

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

    public void ShowMissingIngredients(RecipeSO recipe)
    {
        RefreshIngredients();
        Debug.Log($"Not enough ingredients for recipe: {recipe.name}");
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
    // INPUT
    // ========================================================

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        ResetProgress();
        gameObject.SetActive(false);
    }

    public void ResetProgress()
    {
        progressUI.SetVisible(false);
        progressUI.UpdateProgress(0f);
    }

}
