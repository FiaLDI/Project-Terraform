using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    // Ссылка на ScriptableObject предмета
    [SerializeField] private Item itemData;
    // Количество этого предмета в данном слоте
    [SerializeField] private int amount;

    public int ammoInMagazine; //временная переменная, потом надо будет убрать
    // Свойства для безопасного доступа к данным
    public Item ItemData => itemData;
    public int Amount => amount;

    // Конструктор для создания пустого слота
    public InventorySlot()
    {
        itemData = null;
        amount = 0;
    }

    // Конструктор для создания слота с предметом
    public InventorySlot(Item item, int quantity, int ammoCount = -1)
    {
        itemData = item;
        amount = quantity;

        if (itemData is Weapon weaponData)
        {
            // Если передан корректный счетчик патронов, используем его.
            // Иначе - заряжаем полный магазин.
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
    // Метод для очистки слота
    public void ClearSlot()
    {
        itemData = null;
        amount = 0;
    }
    // Метод для полного обновления данных в слоте
    public void UpdateSlotDataOld(Item item, int quantity)
    {
        itemData = item;
        amount = quantity;
    }
    // Метод для добавления количества к существующему стаку
    public void AddToStack(int value)
    {
        amount += value;
    }

    // Метод для уменьшения количества в стаке
    public void RemoveFromStack(int value)
    {
        amount -= value;
    }
}