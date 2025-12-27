using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Items.Domain;
using Features.Items.Data;

public class UpgradeGlowButtonUI : MonoBehaviour
{
    [Header("Glow Button")]
    [SerializeField] private PolygonGlowButton glowButton;

    [Header("Item UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI levelText;

    private InventorySlotRef slotRef;

    private ItemInstance inst;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController ui;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(ItemInstance inst, UpgradeRecipeSO recipe, UpgradeStationUIController ui, InventorySlotRef slotRef)
    {
        this.inst = inst;
        this.recipe = recipe;
        this.ui = ui;
        this.slotRef = slotRef;
    

        RefreshVisuals();

        // -------- BUTTON ACTION ----------
        glowButton.onClick.RemoveAllListeners();
        glowButton.onClick.AddListener(OnClick);
    }

    // ======================================================
    // VISUALS
    // ======================================================

    public void RefreshVisuals()
    {
        if (inst == null || inst.itemDefinition == null)
            return;

        Item def = inst.itemDefinition;

        if (icon != null)
            icon.sprite = def.icon;

        if (title != null)
            title.text = def.itemName;

        int maxLv = def.upgrades?.Length ?? 0;
        if (levelText != null)
            levelText.text = $"Lv {inst.level}/{maxLv}";

        // если уже максимум — выключаем кнопку
        bool canUpgrade = inst.level < maxLv;
        SetInteractable(canUpgrade);
    }

    // ======================================================
    // BUTTON
    // ======================================================

    private void OnClick()
    {
        if (inst == null || recipe == null || ui == null)
            return;

        ui.OnUpgradeItemSelected(inst, recipe, slotRef);
    }

    // ======================================================
    // STATE
    // ======================================================

    public void SetInteractable(bool value)
    {
        if (glowButton != null)
            glowButton.SetInteractable(value);
    }
}
