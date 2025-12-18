using Features.Items.Data;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public Item itemData;
    public int quantity = 1;
    public bool isWorldObject = true;
   
    private void OnValidate()
    {
        if (itemData != null)
        {
            gameObject.name = itemData.name + " (" + quantity + ")";

            if (!itemData.isStackable)
            {
                quantity = 1;
            }else if (quantity > itemData.maxStackAmount)
            {
                quantity = itemData.maxStackAmount;
            }
            if (quantity < 1)
            {
                quantity = 1;
            }
        }
    }
}