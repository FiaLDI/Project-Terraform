using UnityEngine;

[CreateAssetMenu(menuName = "Items/Upgrade Data")]
public class ItemUpgradeData : ScriptableObject
{
    [HideInInspector]
    public int Level;

    [Header("UI")]
    public Sprite UpgradedIcon;

    [Header("Stat Bonuses")]
    public ItemStat[] bonusStats;
}
