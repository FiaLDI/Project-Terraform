using UnityEngine;

public enum ItemType { Resource, Tool, Weapon, Ammo, Quest, Recource }

public abstract class Item : ScriptableObject
{
    [Header("Base Stats")]
    public ItemStat[] baseStats;

    [Header("Upgrades")]
    public ItemUpgradeData[] upgrades;
    public int currentLevel = 0;

    [Header("In-Game Model")]
    public GameObject worldPrefab;

    [Header("Item Info")]
    public int id;
    public string itemName;
    [TextArea(4, 4)]
    public string description;
    public Sprite icon;

    [Header("Stacking")]
    public bool isStackable;
    public int maxStackAmount = 1;

    [Header("General")]
    public ItemType itemType;

    [Header("Quest Integration")]
    public bool isQuestItem = false;
    public string questId = "";
    public int requiredAmount = 1;

    private void OnValidate()
    {
        if (!isStackable)
            maxStackAmount = 1;

        if (upgrades != null)
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] != null)
                    upgrades[i].Level = i + 1;
            }
        }
    }

    public ItemUpgradeData CurrentUpgrade =>
        (upgrades != null && currentLevel < upgrades.Length)
            ? upgrades[currentLevel]
            : null;

    public ItemUpgradeData NextUpgrade =>
        (upgrades != null && currentLevel + 1 < upgrades.Length)
            ? upgrades[currentLevel + 1]
            : null;
}
