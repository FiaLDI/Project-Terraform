using Features.Items.Data;
using Features.Items.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private InventorySlotRef slotRef;
    private ItemInstance inst;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController ui;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(
        ItemInstance inst,
        UpgradeRecipeSO recipe,
        UpgradeStationUIController ui,
        InventorySlotRef slotRef)
    {
        this.inst    = inst;
        this.recipe  = recipe;
        this.ui      = ui;
        this.slotRef = slotRef;

        RefreshVisuals();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    // ======================================================
    // VISUALS
    // ======================================================

    public void RefreshVisuals()
    {
        if (inst == null || inst.itemDefinition == null)
            return;

        Item def = inst.itemDefinition;

        // ICON
        if (icon != null)
            icon.sprite = def.icon;

        // TITLE
        if (title != null)
            title.text = def.itemName;

        int maxLv = def.upgrades?.Length ?? 0;

        // LEVEL TEXT
        if (levelText != null)
        {
            if (inst.level >= maxLv && maxLv > 0)
                levelText.text = "MAX";
            else
                levelText.text = $"Lv {inst.level}/{maxLv}";
        }

        // INTERACTABLE
        bool canUpgrade = maxLv > 0 && inst.level < maxLv;
        if (button != null)
            button.interactable = canUpgrade;
    }

    // ======================================================
    // BUTTON
    // ======================================================

    private void OnClick()
    {
        if (inst == null || recipe == null || ui == null)
            return;

        var def = inst.itemDefinition;
        int maxLv = def?.upgrades?.Length ?? 0;

        Debug.Log($"[UpgradeItemButtonUI] Click item={def?.id} lvl={inst.level}/{maxLv}");

        if (inst.level >= maxLv)
            return;

        ui.OnUpgradeItemSelected(inst, recipe, slotRef);
    }

}
