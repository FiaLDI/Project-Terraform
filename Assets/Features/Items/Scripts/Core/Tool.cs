using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Items/Tool")]
public class Tool : Item
{
    [Header("Tool Settings")]
    public string toolId;
    public string[] canHarvestIds;
    public float baseHarvestSpeed = 1f;

    private void OnValidate()
    {
        itemType = ItemType.Tool;
    }
}
