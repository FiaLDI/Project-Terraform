using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> bag = new();

    public ItemSaveData leftHand;
    public ItemSaveData rightHand;
}

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public int quantity;
    public int level;
}
