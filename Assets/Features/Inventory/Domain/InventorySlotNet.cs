using FishNet.Serializing;

public struct InventorySlotNet
{
    public string itemId;
    public int quantity;
    public int level;

    public bool IsEmpty => string.IsNullOrEmpty(itemId);
}

