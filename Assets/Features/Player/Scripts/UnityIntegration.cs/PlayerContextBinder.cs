using UnityEngine;
using Features.Inventory;
using Features.Player;
using Features.Inventory.UnityIntegration;

public class PlayerContextBinder : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;

    private void Awake()
    {
        LocalPlayerContext.SetInventory(inventoryManager);
    }
}
