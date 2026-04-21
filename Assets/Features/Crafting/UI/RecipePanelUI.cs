using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Inventory.Domain;

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
    [SerializeField] private Button cancelButton;

    [Header("Progress UI")]
    [SerializeField] private CraftingProgressUI progressUI;

    private IInventoryContext inventory;
    private InventorySlotRef? currentSlot;


    private RecipeSO currentRecipe;
    private ItemInstance currentInstance;
    private Action currentAction;
    private Action currentCancelAction;

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

    public void SetCancelAction(Action action)
    {
        currentCancelAction = action;

        if (cancelButton == null)
            return;

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => currentCancelAction?.Invoke());
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

        SetProcessingState(false);
        actionButton.onClick.RemoveAllListeners();
    }

    // ========================================================
    // UPGRADE VIEW
    // ========================================================

    public void ShowUpgradeRecipe(ItemInstance inst, RecipeSO recipe, InventorySlotRef slotRef)
    {
        currentRecipe = recipe;
        currentSlot   = slotRef;

        gameObject.SetActive(true);

        var def = inst.itemDefinition;
        var next = def.upgrades[inst.level];

        icon.sprite = def.icon;
        title.text = def.itemName + " — Upgrade";

        upgradeInfoText.text =
            $"Current: Lv {inst.level}\n" +
            $"Next: Lv {inst.level + 1}\n" +
            FormatUpgrade(next);


        upgradePreviewIcon.sprite =
            next.UpgradedIcon != null ? next.UpgradedIcon : def.icon;

        RefreshIngredients();
        SetProcessingState(false);
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
        CancelInvoke(nameof(HideProgress));
        SetProcessingState(true);
        progressUI.UpdateProgress(0f);
    }

    public void UpdateProgress(float t)
    {
        progressUI.UpdateProgress(t);
    }

    public void ProcessComplete()
    {
        progressUI.UpdateProgress(1f);
        SetCancelButtonVisible(false);
        Invoke(nameof(HideProgress), 0.2f);
    }

    private void HideProgress()
    {
        SetProcessingState(false);
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
        CancelInvoke(nameof(HideProgress));
        SetProcessingState(false);
        progressUI.UpdateProgress(0f);
    }

    public void Clear()
    {
        currentRecipe = null;
        currentSlot   = null;
        currentAction = null;
        currentCancelAction = null;

        Close();
    }

    public void RefreshUpgradeInfo()
    {
        if (currentSlot == null || currentRecipe == null)
            return;

        var slotRef = currentSlot.Value;

        // ← ДОБАВИТь: достаём инвентарь
        var invMgr = inventory as InventoryManager;
        if (invMgr == null || invMgr.Model == null)
            return;

        // ← ДОБАВИТь: читаем свежий слот из модели
        InventorySlot slot = slotRef.Section switch
        {
            InventorySection.Bag       => invMgr.Model.main[slotRef.Index],
            InventorySection.ActiveSlot0 => invMgr.Model.activeSlot0,
            InventorySection.ActiveSlot1 => invMgr.Model.activeSlot1,
            InventorySection.ActiveSlot2 => invMgr.Model.activeSlot2,
            _ => null
        };

        if (slot == null || slot.item == null || slot.item.IsEmpty)
            return;

        var inst = slot.item;  // ← ВСЕГДА актуальный

        var def = inst.itemDefinition;
        if (def == null || def.upgrades == null)
            return;

        if (inst.level >= def.upgrades.Length)
        {
            Clear();
            return;
        }

        var next = def.upgrades[inst.level];

        upgradeInfoText.text =
            $"Current: Lv {inst.level}\n" +
            $"Next: Lv {inst.level + 1}\n" +
            FormatUpgrade(next);


        upgradePreviewIcon.sprite =
            next.UpgradedIcon != null ? next.UpgradedIcon : def.icon;
    }

    public void ShowMissingIngredients(RecipeSO recipe)
    {
        currentRecipe = recipe;

        RefreshIngredients();

        Debug.Log($"[RecipePanelUI] Not enough ingredients for recipe: {recipe.name}");
    }

    private string FormatUpgrade(ItemUpgradeData upgrade)
    {
        if (upgrade == null || upgrade.levelBuffs == null)
            return "";

        System.Text.StringBuilder sb = new();

        foreach (var buff in upgrade.levelBuffs)
        {
            if (buff == null)
                continue;

            foreach (var effect in buff.effects)
            {
                if (effect is AddStatEffectSO add)
                {
                    string sign = add.value >= 0 ? "+" : "";
                    sb.AppendLine($"{sign}{add.value} {add.statId}");
                }
                else if (effect is MultiplyStatEffectSO mult)
                {
                    float percent = (mult.Multiplier - 1f) * 100f;
                    string sign = percent >= 0 ? "+" : "";
                    sb.AppendLine($"{sign}{percent:0.#}% {mult.StatId}");
                }
            }
        }

        return sb.ToString();
    }

    private void SetProcessingState(bool isProcessing)
    {
        if (progressUI != null)
            progressUI.SetVisible(isProcessing);

        if (actionButton != null)
            actionButton.interactable = !isProcessing;

        SetCancelButtonVisible(isProcessing);
    }

    private void SetCancelButtonVisible(bool isVisible)
    {
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(isVisible);
    }


}
