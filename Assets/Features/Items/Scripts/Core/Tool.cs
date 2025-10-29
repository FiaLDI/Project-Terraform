using UnityEngine;

[CreateAssetMenu(menuName = "Items/Tool")]
public class Tool : Item
{
    [Header("Tool Settings")]
    public string toolId;
    public string[] canHarvestIds; // id предметов, которые может добывать
    public float baseHarvestSpeed = 1f;

    private void OnValidate()
    {
        itemType = ItemType.Tool;
    }
}