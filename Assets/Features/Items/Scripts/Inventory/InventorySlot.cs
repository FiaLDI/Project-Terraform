using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private Item itemData;
    [SerializeField] private int amount;

    public int ammoInMagazine;
    public event System.Action OnChanged;
    public Item ItemData => itemData;
    public int Amount => amount;

    public InventorySlot()
    {
        itemData = null;
        amount = 0;
    }

    public InventorySlot(Item item, int quantity, int ammoCount = -1)
    {
        itemData = item;
        amount = quantity;

        if (itemData is Weapon weaponData)
        {
            ammoInMagazine = (ammoCount != -1) ? ammoCount : weaponData.magazineSize;
        }
    }

    public void UpdateSlotData(Item item, int quantity, int ammoCount = -1)
    {
        itemData = item;
        amount = quantity;

        if (itemData is Weapon weaponData)
        {
            ammoInMagazine = (ammoCount != -1) ? ammoCount : weaponData.magazineSize;
        }
        else
        {
            ammoInMagazine = 0;
        }
    }

    public void ClearSlot()
    {
        itemData = null;
        amount = 0;
    }

    public void UpdateSlotDataOld(Item item, int quantity)
    {
        itemData = item;
        amount = quantity;
    }

    public void AddToStack(int value)
    {
        amount += value;
    }

    public void RemoveFromStack(int value)
    {
        amount -= value;
    }

    public void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}