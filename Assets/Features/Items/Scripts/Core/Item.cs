using UnityEngine;

public enum ItemType { Resource, Tool, Weapon, Ammo, Quest, Recource }

public abstract class Item : ScriptableObject
{
    [Header("Base Stats")]
    public ItemStat[] baseStats;

    [Header("Upgrades")]
    public ItemUpgradeData[] upgrades;

    /// <summary>
    /// 0 = базовый, 1 = upgrades[0], 2 = upgrades[1], ...
    /// </summary>
    [Tooltip("0 = базовый, 1 = upgrades[0], 2 = upgrades[1] ...")]
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
    }

    public ItemUpgradeData CurrentUpgrade =>
        (currentLevel >= 1 && upgrades != null && currentLevel - 1 < upgrades.Length)
            ? upgrades[currentLevel - 1]
            : null;

    public ItemUpgradeData NextUpgrade =>
        (upgrades != null && currentLevel < upgrades.Length)
            ? upgrades[currentLevel]
            : null;

    public float GetTotalStat(ItemStatType type)
    {
        float v = 0;

        foreach (var b in baseStats)
            if (b.stat == type)
                v += b.value;

        for (int i = 1; i <= currentLevel; i++)
        {
            int idx = i - 1;
            if (idx < upgrades.Length)
            {
                foreach (var bonus in upgrades[idx].bonusStats)
                    if (bonus.stat == type)
                        v += bonus.value;
            }
        }

        return v;
    }
}
