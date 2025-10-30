using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static bool isInventoryOpen;
    public static InventoryManager instance;
    public static event System.Action<Item, int> OnItemAdded;

    [Header("Hotbar")]
    [SerializeField] private RectTransform selectionHighlight;
    public int selectedHotbarIndex = 0;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainInventoryPanel;
    [SerializeField] private GameObject hotbarPanel;

    [Header("Inventory Data")]
    [SerializeField] private List<InventorySlot> mainInventorySlots = new List<InventorySlot>();
    [SerializeField] private List<InventorySlot> hotbarSlots = new List<InventorySlot>();

    private InventorySlotUI[] mainInventoryUI;
    private InventorySlotUI[] hotbarUI;

    //[Header("World Interaction")]
    //[SerializeField] 
    private Transform playerTransform;

    [Header("Drag & Drop")]
    [SerializeField] public Image draggableItem;
    private InventorySlot draggedSlot;

    public const int MAIN_INVENTORY_SIZE = 48;
    public const int HOTBAR_SIZE = 2;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        for (int i = 0; i < MAIN_INVENTORY_SIZE; i++) mainInventorySlots.Add(new InventorySlot());
        for (int i = 0; i < HOTBAR_SIZE; i++) hotbarSlots.Add(new InventorySlot());

        mainInventoryUI = mainInventoryPanel.GetComponentsInChildren<InventorySlotUI>();
        hotbarUI = hotbarPanel.GetComponentsInChildren<InventorySlotUI>();
    }

    private void Start()
    {
        // Безопасно ищем Player при запуске
        StartCoroutine(WaitForPlayerReference());
        UpdateAllUI();
        mainInventoryPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SelectHotbarSlot(0);
    }
    /// <summary>
    /// Безопасно ждёт, пока на сцене появится объект с тегом Player, и сохраняет его Transform.
    /// </summary>
    private IEnumerator WaitForPlayerReference()
    {
        // Если ссылка уже есть — выходим
        if (playerTransform != null) yield break;

        GameObject playerObj = null;
        // Ожидаем появления объекта Player
        while (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                Debug.Log("[InventoryManager] Player найден и привязан успешно.");
                yield break;
            }
            yield return new WaitForSeconds(0.1f); // проверяем каждые 100 мс
        }
    }
    private Vector3 GetSafeDropPosition()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[InventoryManager] Игрок ещё не найден, выбрасывание предмета невозможно.");
            return Vector3.zero;
        }
        return playerTransform.position + playerTransform.forward * 1.5f + playerTransform.up * 2f;
    }
    public bool IsOpen => mainInventoryPanel.activeSelf;
    public void SetOpen(bool state)
    {
        mainInventoryPanel.SetActive(state);
        isInventoryOpen = state;

        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SIZE) return;
        selectedHotbarIndex = index;
        selectionHighlight.position = hotbarUI[selectedHotbarIndex].transform.position;
        UpdateEquippedItem();
    }

    public void DropItemFromSelectedSlot() => DropItem(selectedHotbarIndex);
    public void DropFullStackFromSelectedSlot() => DropFullStackFromHotbar(selectedHotbarIndex);
    // Новый главный метод
    public void AddItem(Item itemToAdd, int quantityToAdd, int ammoInMagazine)
    {
        // --- ЛОГИКА ДЛЯ НЕСТАКУЕМЫХ ПРЕДМЕТОВ ---
        if (!itemToAdd.isStackable)
        {
            for (int i = 0; i < quantityToAdd; i++)
            {
                InventorySlot emptySlot = hotbarSlots.Find(slot => slot.ItemData == null) ?? mainInventorySlots.Find(slot => slot.ItemData == null);

                if (emptySlot != null)
                {
                    // Передаем патроны при обновлении слота
                    emptySlot.UpdateSlotData(itemToAdd, 1, ammoInMagazine);

                }
                else
                {
                    Debug.Log("Инвентарь полон, не удалось добавить все экземпляры " + itemToAdd.itemName);
                    break;
                }
            }
        }
        else // --- ЛОГИКА ДЛЯ СТАКУЕМЫХ ПРЕДМЕТОВ ---
        {
            //(Ваша существующая логика для стаков остается здесь без изменений):
            int quantityRemaining = quantityToAdd;

            // 1. Проходим по существующим стакам и пытаемся их дополнить
            foreach (var slot in hotbarSlots)
            {
                if (slot.ItemData != null && slot.ItemData.id == itemToAdd.id && slot.Amount < itemToAdd.maxStackAmount)
                {
                    int spaceAvailable = itemToAdd.maxStackAmount - slot.Amount;
                    int amountToTransfer = Mathf.Min(quantityRemaining, spaceAvailable);

                    slot.AddToStack(amountToTransfer);
                    quantityRemaining -= amountToTransfer;

                    if (quantityRemaining <= 0) break;
                }
            }

            if (quantityRemaining > 0)
            {
                foreach (var slot in mainInventorySlots)
                {
                    if (slot.ItemData != null && slot.ItemData.id == itemToAdd.id && slot.Amount < itemToAdd.maxStackAmount)
                    {
                        int spaceAvailable = itemToAdd.maxStackAmount - slot.Amount;
                        int amountToTransfer = Mathf.Min(quantityRemaining, spaceAvailable);

                        slot.AddToStack(amountToTransfer);
                        quantityRemaining -= amountToTransfer;

                        if (quantityRemaining <= 0) break;
                    }
                }
            }
            // Но в конце, где вы ищете пустой слот:
            while (quantityRemaining > 0)
            {
                InventorySlot emptySlot = hotbarSlots.Find(slot => slot.ItemData == null) ?? mainInventorySlots.Find(slot => slot.ItemData == null);

                if (emptySlot == null)
                {
                    Debug.Log("Инвентарь полон, не удалось добавить все предметы.");
                    break;
                }

                int amountToTransfer = Mathf.Min(quantityRemaining, itemToAdd.maxStackAmount);
                // Передаем патроны при обновлении слота
                emptySlot.UpdateSlotData(itemToAdd, amountToTransfer, ammoInMagazine);
                quantityRemaining -= amountToTransfer;
            }
        }
        OnItemAdded?.Invoke(itemToAdd, quantityToAdd);
        UpdateAllUI();
        UpdateEquippedItem();
    }

    // Вспомогательный метод, вызывающий главный
    public void AddItem(Item itemToAdd, int quantityToAdd)
    {
        int initialAmmo = (itemToAdd is Weapon w) ? w.magazineSize : 0;
        AddItem(itemToAdd, quantityToAdd, initialAmmo);
    }

    // Вспомогательный метод для добавления одной штуки
    public void AddItem(Item itemToAdd)
    {
        AddItem(itemToAdd, 1);
    }
    // Обновляет все UI слоты на основе данных
    private void UpdateAllUI()
    {
        for (int i = 0; i < mainInventoryUI.Length; i++)
        {
            mainInventoryUI[i].UpdateSlot(mainInventorySlots[i]);
        }

        for (int i = 0; i < hotbarUI.Length; i++)
        {
            hotbarUI[i].UpdateSlot(hotbarSlots[i]);
        }
    }
    public void DropItem(int hotbarIndex)
    {
        // Проверяем, есть ли предмет в указанном слоте хотбара
        InventorySlot slotToDrop = hotbarSlots[hotbarIndex]; // Для удобства сохраним слот в переменную
        if (slotToDrop.ItemData == null)
        {
            Debug.Log("Слот пуст, нечего выбрасывать.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[InventoryManager] PlayerTransform отсутствует. Пропускаем выброс предмета.");
            return;
        }

        Item itemToDrop = slotToDrop.ItemData;
        Vector3 dropPosition = GetSafeDropPosition();
        if (slotToDrop.ItemData == null)
        {
            Debug.Log("Слот пуст, нечего выбрасывать.");
            return;
        }

        //Item itemToDrop = slotToDrop.ItemData;

        // Определяем позицию для спавна предмета перед игроком
        //Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f+playerTransform.up*2;

        // Используем префаб, указанный в самом предмете!
        GameObject spawnedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        // Передаем созданному объекту все данные о предмете, который мы выбрасываем
        ItemObject itemObject = spawnedObject.GetComponent<ItemObject>();
        itemObject.itemData = itemToDrop;
        itemObject.quantity = 1; // Мы выбрасываем одну штуку

        // Сохраняем количество патронов из слота в 3D-объект
        //itemObject.ammoInMagazine = slotToDrop.ammoInMagazine;

        // Включаем физику для выброшенного предмета
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Уменьшаем количество предмета в слоте
        slotToDrop.RemoveFromStack(1);

        // Если предметов в слоте не осталось, очищаем его
        if (slotToDrop.Amount <= 0)
        {
            slotToDrop.ClearSlot();
        }

        // Обновляем UI
        UpdateAllUI();
        // И сразу после этого обновляем предмет в руке!
        UpdateEquippedItem();
    }
    public void DropItemOld(int hotbarIndex)
    {
        // Проверяем, есть ли предмет в указанном слоте хотбара
        if (hotbarSlots[hotbarIndex].ItemData == null)
        {
            Debug.Log("Слот пуст, нечего выбрасывать.");
            return;
        }
        Item itemToDrop = hotbarSlots[hotbarIndex].ItemData;
        // Определяем позицию для спавна предмета перед игроком
        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f;

        // Создаем объект предмета в мире из префаба
        //GameObject spawnedObject = Instantiate(worldItemPrefab, dropPosition, Quaternion.identity);
        // Используем префаб, указанный в самом предмете!
        GameObject spawnedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);
        // Передаем созданному объекту данные о предмете, который мы выбрасываем
        spawnedObject.GetComponent<ItemObject>().itemData = itemToDrop;

        // Включаем физику для выброшенного предмета
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            // Можно также явно включить гравитацию, если она была отключена на префабе
            rb.useGravity = true;
        }
        // Уменьшаем количество предмета в слоте
        hotbarSlots[hotbarIndex].RemoveFromStack(1);

        // Если предметов в слоте не осталось, очищаем его
        if (hotbarSlots[hotbarIndex].Amount <= 0)
        {
            hotbarSlots[hotbarIndex].ClearSlot();
        }

        // Обновляем UI
        UpdateAllUI();
        // И сразу после этого обновляем предмет в руке!
        UpdateEquippedItem();
    }
    public bool HasItem(Item itemToCheck)
    {
        if (itemToCheck == null) return false;

        // Ищем предмет во всем инвентаре
        foreach (var slot in hotbarSlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == itemToCheck.id) return true;
        }
        foreach (var slot in mainInventorySlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == itemToCheck.id) return true;
        }
        return false;
    }
    // Вызывается, когда мы начинаем тащить предмет
    public void OnDragBegin(InventorySlot slot)
    {
        if (slot.ItemData != null)
        {
            draggedSlot = slot;
            draggableItem.sprite = slot.ItemData.icon;
            draggableItem.gameObject.SetActive(true);
        }
    }

    // Вызывается, когда мы отпускаем предмет
    public void OnDragEnd()
    {
        if (draggedSlot != null)
        {
            draggableItem.gameObject.SetActive(false);
            draggedSlot = null;
        }
    }

    // Вызывается, когда мы бросаем предмет на другой слот
    public void OnDrop(InventorySlot dropSlot)
    {
        if (draggedSlot == null) return;

        // Пока что реализуем простой обмен местами
        // Более сложная логика (стакинг) может быть добавлена позже
        Item draggedItemData = draggedSlot.ItemData;
        int draggedItemAmount = draggedSlot.Amount;
        int draggedAmmo = draggedSlot.ammoInMagazine;

        // Перемещаем данные из dropSlot в draggedSlot
        draggedSlot.UpdateSlotData(dropSlot.ItemData, dropSlot.Amount, draggedSlot.ammoInMagazine);

        // Перемещаем данные из draggedSlot (которые мы сохранили) в dropSlot
        dropSlot.UpdateSlotData(draggedItemData, draggedItemAmount, draggedAmmo);

        UpdateAllUI();
    }

    public void UpdateDraggableItemPosition(Vector2 newPosition)
    {
        if (draggedSlot != null)
        {
            draggableItem.rectTransform.position = newPosition;
        }
    }

    public void UpdateEquippedItem()
    {
        // Берём предмет из текущего выбранного слота
        Item selectedItem = hotbarSlots[selectedHotbarIndex].ItemData;
        // И просим EquipmentManager его экипировать (или убрать, если слот пуст)
        EquipmentManager.instance.EquipItem(selectedItem);
    }


    public bool HasAmmo(Ammo ammoType)
    {
        // Ищем нужные патроны во всем инвентаре (хотбар + основной)
        foreach (var slot in hotbarSlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == ammoType.id) return true;
        }
        foreach (var slot in mainInventorySlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == ammoType.id) return true;
        }
        return false;
    }
    public int GetAmmoCount(Ammo ammoType)
    {
        int totalAmount = 0;
        if (ammoType == null) return 0;

        foreach (var slot in hotbarSlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == ammoType.id)
                totalAmount += slot.Amount;
        }
        foreach (var slot in mainInventorySlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == ammoType.id)
                totalAmount += slot.Amount;
        }
        return totalAmount;
    }

    public void ConsumeAmmo(Ammo ammoType, int amountToConsume)
    {
        int amountLeft = amountToConsume;

        // Ищем в обратном порядке, чтобы тратить из последних слотов
        for (int i = mainInventorySlots.Count - 1; i >= 0; i--)
        {
            if (mainInventorySlots[i].ItemData != null && mainInventorySlots[i].ItemData.id == ammoType.id)
            {
                int amountInSlot = mainInventorySlots[i].Amount;
                int amountToTake = Mathf.Min(amountLeft, amountInSlot);

                mainInventorySlots[i].RemoveFromStack(amountToTake);
                if (mainInventorySlots[i].Amount <= 0)
                {
                    mainInventorySlots[i].ClearSlot();
                }
                amountLeft -= amountToTake;
                if (amountLeft <= 0) break;
            }
        }

        if (amountLeft > 0)
        {
            for (int i = hotbarSlots.Count - 1; i >= 0; i--)
            {
                if (hotbarSlots[i].ItemData != null && hotbarSlots[i].ItemData.id == ammoType.id)
                {
                    int amountInSlot = hotbarSlots[i].Amount;
                    int amountToTake = Mathf.Min(amountLeft, amountInSlot);

                    hotbarSlots[i].RemoveFromStack(amountToTake);
                    if (hotbarSlots[i].Amount <= 0)
                    {
                        hotbarSlots[i].ClearSlot();
                    }
                    amountLeft -= amountToTake;
                    if (amountLeft <= 0) break;
                }
            }
        }

        UpdateAllUI();
    }
    public void ConsumeAmmoOld(Ammo ammoType)
    {
        // Ищем слот с патронами и уменьшаем их количество
        // Ищем в обратном порядке, чтобы сначала тратить патроны из последних слотов
        for (int i = mainInventorySlots.Count - 1; i >= 0; i--)
        {
            if (mainInventorySlots[i].ItemData != null && mainInventorySlots[i].ItemData.id == ammoType.id)
            {
                mainInventorySlots[i].RemoveFromStack(1);
                if (mainInventorySlots[i].Amount <= 0)
                {
                    mainInventorySlots[i].ClearSlot();
                }
                UpdateAllUI();
                return;
            }
        }

        for (int i = hotbarSlots.Count - 1; i >= 0; i--)
        {
            if (hotbarSlots[i].ItemData != null && hotbarSlots[i].ItemData.id == ammoType.id)
            {
                hotbarSlots[i].RemoveFromStack(1);
                if (hotbarSlots[i].Amount <= 0)
                {
                    hotbarSlots[i].ClearSlot();
                }
                UpdateAllUI();
                return;
            }
        }
    }
    public void DropFullStackFromHotbar(int hotbarIndex)
    {
        // Проверяем, есть ли предмет в указанном слоте хотбара
        InventorySlot slotToDrop = hotbarSlots[hotbarIndex];
        if (slotToDrop.ItemData == null)
        {
            Debug.Log("Слот пуст, нечего выбрасывать.");
            return;
        }

        Item itemToDrop = slotToDrop.ItemData;
        int quantityToDrop = slotToDrop.Amount; // Берем всё количество из слота

        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f;

        GameObject spawnedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        // Устанавливаем и тип предмета, и его количество
        ItemObject itemObject = spawnedObject.GetComponent<ItemObject>();
        itemObject.itemData = itemToDrop;
        itemObject.quantity = quantityToDrop;
        //itemObject.ammoInMagazine = slotToDrop.ammoInMagazine;
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Полностью очищаем слот
        slotToDrop.ClearSlot();

        UpdateAllUI();
        UpdateEquippedItem();
    }
    public void HandleItemDropFromDrag()
    {
        // Проверяем, действительно ли мы что-то тащим
        if (draggedSlot == null || draggedSlot.ItemData == null)
        {
            return;
        }

        Item itemToDrop = draggedSlot.ItemData;
        int quantityToDrop = draggedSlot.Amount;

        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f;

        GameObject spawnedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        ItemObject itemObject = spawnedObject.GetComponent<ItemObject>();
        itemObject.itemData = itemToDrop;
        itemObject.quantity = quantityToDrop;

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Очищаем слот, из которого мы тащили предмет
        draggedSlot.ClearSlot();

        UpdateAllUI();
        UpdateEquippedItem();
    }
    // Получает количество патронов в магазине для предмета в указанном слоте хотбара
    public int GetMagazineAmmoForSlot(int hotbarIndex)
    {
        return hotbarSlots[hotbarIndex].ammoInMagazine;
    }

    // Устанавливает количество патронов в магазине для предмета в указанном слоте хотбара
    public void SetMagazineAmmoForSlot(int hotbarIndex, int ammoCount)
    {
        hotbarSlots[hotbarIndex].ammoInMagazine = ammoCount;
    }
}