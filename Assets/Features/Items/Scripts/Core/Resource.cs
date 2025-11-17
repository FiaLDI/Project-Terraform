using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Items/Resource")]
public class Resource : Item
{
    private void OnValidate()
    {
        itemType = ItemType.Recource;
    }
}
