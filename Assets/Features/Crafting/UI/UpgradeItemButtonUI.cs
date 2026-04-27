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
    private UpgradeStationUI ui;

    public void Init(
        ItemInstance inst,
        UpgradeRecipeSO recipe,
        UpgradeStationUI ui,
        InventorySlotRef slotRef)
    {
        this.inst = inst;
        this.recipe = recipe;
        this.ui = ui;
        this.slotRef = slotRef;

        RefreshVisuals();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

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
        {
            if (inst.level >= maxLv && maxLv > 0)
                levelText.text = "MAX";
            else
                levelText.text = $"Lv {inst.level}/{maxLv}";
        }

        bool canUpgrade = maxLv > 0 && inst.level < maxLv;
        if (button != null)
            button.interactable = canUpgrade;
    }

    private void OnClick()
    {
        if (inst == null || recipe == null || ui == null)
            return;

        Item def = inst.itemDefinition;
        int maxLv = def?.upgrades?.Length ?? 0;

        Debug.Log($"[UpgradeItemButtonUI] Click item={def?.id} lvl={inst.level}/{maxLv}");

        if (inst.level >= maxLv)
            return;

        ui.OnUpgradeItemSelected(inst, recipe, slotRef);
    }
}
