using FishNet.Serializing;

public static class InventorySlotNetSerializers
{
    public static void WriteInventorySlotNet(this Writer writer, InventorySlotNet value)
    {
        writer.WriteString(value.itemId);
        writer.WriteInt32(value.quantity);
        writer.WriteInt32(value.level);
    }

    public static InventorySlotNet ReadInventorySlotNet(this Reader reader)
    {
        return new InventorySlotNet
        {
            itemId = reader.ReadString(),
            quantity = reader.ReadInt32(),
            level = reader.ReadInt32()
        };
    }
}
