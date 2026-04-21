using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> bag = new();

    public ItemSaveData activeSlot0;
    public ItemSaveData activeSlot1;
    public ItemSaveData activeSlot2;

    public int activeSlotIndex;
}

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public int quantity;
    public int level;
}
