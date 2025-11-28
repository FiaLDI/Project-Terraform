using UnityEngine;

[CreateAssetMenu(menuName = "Items/Upgrade Data")]
public class ItemUpgradeData : ScriptableObject
{
    [Header("UI")]
    public int Level;
    public Sprite UpgradedIcon;

    [Header("Stat Bonuses")]
    public ItemStat[] bonusStats;
}
