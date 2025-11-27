using UnityEngine;

[CreateAssetMenu(menuName = "Items/Upgrade Data")]
public class ItemUpgradeData : ScriptableObject
{
    public int level;                     // Уровень апгрейда
    public float damageBonus;             // Если инструмент/оружие
    public float speedBonus;              // Скорость работы/атаки
    public float energyCostModifier;      // Расход энергии
    public Sprite upgradedIcon;           // Иконка на уровне
}
