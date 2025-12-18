using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Items.Domain;
using Features.Items.Data;

public class UpgradeItemButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private ItemInstance instance;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController controller;

    public void Init(
        ItemInstance inst,
        UpgradeRecipeSO recipe,
        UpgradeStationUIController controller)
    {
        this.instance = inst;
        this.recipe = recipe;
        this.controller = controller;

        RefreshVisuals();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    // ======================================================
    // VISUALS
    // ======================================================

    private void RefreshVisuals()
    {
        if (instance == null || instance.itemDefinition == null)
            return;

        Item def = instance.itemDefinition;

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
            if (instance.level >= maxLv)
                levelText.text = "MAX";
            else
                levelText.text = $"Lv {instance.level}/{maxLv}";
        }

        // INTERACTABLE
        bool canUpgrade = instance.level < maxLv;
        if (button != null)
            button.interactable = canUpgrade;
    }

    // ======================================================
    // BUTTON
    // ======================================================

    private void OnClick()
    {
        if (instance == null || recipe == null || controller == null)
            return;

        var def = instance.itemDefinition;
        int maxLv = def?.upgrades?.Length ?? 0;

        if (instance.level >= maxLv)
            return;

        controller.OnUpgradeItemSelected(instance, recipe);
    }
}
