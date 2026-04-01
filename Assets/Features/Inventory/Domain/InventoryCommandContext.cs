using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using FishNet.Connection;

public class InventoryCommandContext
{
    public InventoryCommandData Command;
    public InventoryManager Inventory;
    public NetworkConnection Sender;
    public InventoryStateNetwork Owner;

    public bool IsValid = true;
}