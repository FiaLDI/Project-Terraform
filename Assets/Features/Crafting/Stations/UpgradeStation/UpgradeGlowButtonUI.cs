using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeGlowButtonUI : MonoBehaviour
{
    [Header("Glow Button")]
    public PolygonGlowButton glowButton;

    [Header("Item UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI level;

    private Item item;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController controller;

    public void Init(Item item, UpgradeRecipeSO recipe, UpgradeStationUIController controller)
    {
        this.item = item;
        this.recipe = recipe;
        this.controller = controller;

        // ---------------------------
        //   VISUAL
        // ---------------------------
        if (item.icon != null)
            icon.sprite = item.icon;

        title.text = item.itemName;

        int maxLv = item.upgrades != null ? item.upgrades.Length : 0;
        level.text = $"Lv {item.currentLevel}/{maxLv}";

        // ---------------------------
        //   Glow Button Action
        // ---------------------------
        glowButton.onClick.RemoveAllListeners();
        glowButton.onClick.AddListener(() =>
        {
            controller.OnUpgradeItemSelected(item, recipe);
        });
    }
}
