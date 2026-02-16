using UnityEngine;
using Features.Buffs.Domain;

[CreateAssetMenu(menuName = "Items/Upgrade Data")]
public class ItemUpgradeData : ScriptableObject
{
    [HideInInspector]
    public int Level;

    [Header("UI")]
    public Sprite UpgradedIcon;

    [Header("Buffs")]
    public BuffSO[] levelBuffs;
}

