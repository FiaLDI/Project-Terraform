using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private Item item;
    private UpgradeRecipeSO recipe;
    private UpgradeStationUIController controller;

    public void Init(Item item, UpgradeRecipeSO recipe, UpgradeStationUIController controller)
    {
        this.item = item;
        this.recipe = recipe;
        this.controller = controller;

        // -------- ICON --------
        if (icon != null)
        {
            icon.sprite = item.icon != null ? item.icon : null;
        }

        // -------- TITLE --------
        if (title != null)
        {
            title.text = item.itemName;
        }

        // -------- LEVEL --------
        if (levelText != null)
        {
            int current = item.currentLevel;
            int max = item.upgrades != null ? item.upgrades.Length : 0;

            levelText.text = $"Lv {current}/{max}";
        }

        // -------- CLICK ACTION --------
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            controller.OnUpgradeItemSelected(item, recipe);
        });
    }
}
