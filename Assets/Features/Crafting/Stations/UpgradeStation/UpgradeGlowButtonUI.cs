using Features.Items.Data;
using Features.Items.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private UpgradeStationUI ui;

    public void Init(ItemInstance inst, UpgradeRecipeSO recipe, UpgradeStationUI ui, InventorySlotRef slotRef)
    {
        this.inst = inst;
        this.recipe = recipe;
        this.ui = ui;
        this.slotRef = slotRef;

        RefreshVisuals();

        glowButton.onClick.RemoveAllListeners();
        glowButton.onClick.AddListener(OnClick);
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
            levelText.text = $"Lv {inst.level}/{maxLv}";

        bool canUpgrade = inst.level < maxLv;
        SetInteractable(canUpgrade);
    }

    private void OnClick()
    {
        if (inst == null || recipe == null || ui == null)
            return;

        ui.OnUpgradeItemSelected(inst, recipe, slotRef);
    }

    public void SetInteractable(bool value)
    {
        if (glowButton != null)
            glowButton.SetInteractable(value);
    }
}
