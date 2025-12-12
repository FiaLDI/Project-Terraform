using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Items.Domain;
using Features.Items.Data;

public class UpgradeGlowButtonUI : MonoBehaviour
{
    [Header("Glow Button")]
    public PolygonGlowButton glowButton;

    [Header("Item UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI levelText;

    private ItemInstance instance;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController controller;

    public void Init(ItemInstance inst, UpgradeRecipeSO recipe, UpgradeStationUIController controller)
    {
        this.instance = inst;
        this.recipe = recipe;
        this.controller = controller;

        Item def = inst.itemDefinition;

        // -------- VISUAL ----------
        if (def.icon != null)
            icon.sprite = def.icon;

        title.text = def.itemName;

        int maxLv = def.upgrades != null ? def.upgrades.Length : 0;
        levelText.text = $"Lv {inst.level}/{maxLv}";

        // -------- BUTTON ACTION ----------
        glowButton.onClick.RemoveAllListeners();
        glowButton.onClick.AddListener(() =>
        {
            controller.OnUpgradeItemSelected(inst, recipe);
        });
    }
}
