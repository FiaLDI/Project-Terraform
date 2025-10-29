using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Stats")]
    public float damage;
    public float fireRate;
    [Header("Weapon Settings")]
    public int magazineSize = 12; // Размер магазина
    // Ссылка на тип патронов, которые использует это оружие
    public Ammo requiredAmmoType;
    private void OnValidate()
    {
        itemType = ItemType.Weapon;
    }
}