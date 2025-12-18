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

    private ItemInstance instance;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController controller;

    // ======================================================
    // INIT
    // ======================================================

    public void Init(
        ItemInstance inst,
        UpgradeRecipeSO recipe,
        UpgradeStationUIController controller)
    {
        this.instance = inst;
        this.recipe = recipe;
        this.controller = controller;

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
        if (instance == null || instance.itemDefinition == null)
            return;

        Item def = instance.itemDefinition;

        if (icon != null)
            icon.sprite = def.icon;

        if (title != null)
            title.text = def.itemName;

        int maxLv = def.upgrades?.Length ?? 0;
        if (levelText != null)
            levelText.text = $"Lv {instance.level}/{maxLv}";

        // если уже максимум — выключаем кнопку
        bool canUpgrade = instance.level < maxLv;
        SetInteractable(canUpgrade);
    }

    // ======================================================
    // BUTTON
    // ======================================================

    private void OnClick()
    {
        if (instance == null || recipe == null || controller == null)
            return;

        controller.OnUpgradeItemSelected(instance, recipe);
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
