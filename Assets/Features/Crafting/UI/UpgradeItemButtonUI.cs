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

    public void Init(ItemInstance inst, UpgradeRecipeSO recipe, UpgradeStationUIController controller)
    {
        this.instance = inst;
        this.recipe = recipe;
        this.controller = controller;

        Item def = inst.itemDefinition;

        // ICON
        if (icon != null)
            icon.sprite = def.icon;

        // TITLE
        if (title != null)
            title.text = def.itemName;

        // LEVEL
        if (levelText != null)
        {
            int maxLv = def.upgrades != null ? def.upgrades.Length : 0;
            levelText.text = $"Lv {inst.level}/{maxLv}";
        }

        // ACTION
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            controller.OnUpgradeItemSelected(inst, recipe);
        });
    }
}
