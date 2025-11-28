using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button button;

    private Item item;
    private RecipeSO recipe;
    private UpgradeStationUIController controller;

    public void Init(Item item, RecipeSO recipe, UpgradeStationUIController controller)
    {
        this.item = item;
        this.recipe = recipe;
        this.controller = controller;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        int max = item.upgrades != null ? item.upgrades.Length : 0;
        if (levelText != null)
            levelText.text = $"{item.currentLevel}/{max}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        controller.OnUpgradeItemSelected(item, recipe);
    }
}
