using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static bool isInventoryOpen;
    public static InventoryManager instance;
    public static event System.Action<Item, int> OnItemAdded;

    public static InventoryManager Instance => instance;

    [Header("Hotbar")]
    [SerializeField] private RectTransform selectionHighlight;

    public int selectedHotbarIndex = 0;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainInventoryPanel;
    [SerializeField] private GameObject hotbarPanel;

    [Header("Inventory Data")]
    [SerializeField] private List<InventorySlot> mainInventorySlots = new();
    [SerializeField] private List<InventorySlot> hotbarSlots = new();

    private InventorySlotUI[] mainInventoryUI;
    private InventorySlotUI[] hotbarUI;

    [Header("World Interaction")]
    [SerializeField] private Transform playerTransform;

    [Header("Drag & Drop")]
    [SerializeField] public Image draggableItem;
    private InventorySlot draggedSlot;

    public const int MAIN_INVENTORY_SIZE = 48;
    public const int HOTBAR_SIZE = 2;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < MAIN_INVENTORY_SIZE; i++) mainInventorySlots.Add(new InventorySlot());
        for (int i = 0; i < HOTBAR_SIZE; i++) hotbarSlots.Add(new InventorySlot());

        mainInventoryUI = mainInventoryPanel.GetComponentsInChildren<InventorySlotUI>();
        hotbarUI = hotbarPanel.GetComponentsInChildren<InventorySlotUI>();
    }

    private void Start()
    {
        UpdateAllUI();
        mainInventoryPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SelectHotbarSlot(0);
        StartCoroutine(DelayedHotbarSelection());
    }

    private IEnumerator DelayedHotbarSelection()
    {
        yield return null;
        SelectHotbarSlot(0);
    }

    // ===================================================================
    //  UNIVERSAL ADD ITEM (CRAFT + QUESTS + PICKUPS)
    // ===================================================================

    private void AddItemInternal(Item item, int amount)
    {
        if (item == null || amount <= 0)
            return;

        // ---------------- STACKABLE ----------------
        if (item.isStackable)
        {
            int left = amount;

            // 1. Заполняем стеки в хотбаре
            foreach (var slot in hotbarSlots)
            {
                if (slot.ItemData != null && slot.ItemData.id == item.id && slot.Amount < item.maxStackAmount)
                {
                    int canAdd = Mathf.Min(item.maxStackAmount - slot.Amount, left);
                    slot.AddToStack(canAdd);
                    left -= canAdd;
                    if (left <= 0) goto FINISH;
                }
            }

            // 2. Заполняем стеки в основном инвентаре
            foreach (var slot in mainInventorySlots)
            {
                if (slot.ItemData != null && slot.ItemData.id == item.id && slot.Amount < item.maxStackAmount)
                {
                    int canAdd = Mathf.Min(item.maxStackAmount - slot.Amount, left);
                    slot.AddToStack(canAdd);
                    left -= canAdd;
                    if (left <= 0) goto FINISH;
                }
            }

            // 3. Свободные ячейки
            while (left > 0)
            {
                InventorySlot empty =
                    hotbarSlots.Find(s => s.ItemData == null) ??
                    mainInventorySlots.Find(s => s.ItemData == null);

                if (empty == null)
                    break;

                int addNow = Mathf.Min(left, item.maxStackAmount);
                empty.UpdateSlotData(item, addNow);
                left -= addNow;
            }
        }
        else
        {
            // ---------------- NON-STACKABLE ----------------
            for (int i = 0; i < amount; i++)
            {
                InventorySlot empty =
                    hotbarSlots.Find(s => s.ItemData == null) ??
                    mainInventorySlots.Find(s => s.ItemData == null);

                if (empty == null)
                    break;

                // Сюда мы уже передаём runtime-клон (см. Crafting/Drop)
                empty.UpdateSlotData(item, 1);
            }
        }

    FINISH:

        OnItemAdded?.Invoke(item, amount);
        UpdateAllUI();
        UpdateEquippedItem();
    }


    // ----------------------------------------------------------
    // PUBLIC API
    // ----------------------------------------------------------

    public void AddItem(Item item)
    {
        AddItemInternal(item, 1);
    }

    public void AddItem(Item item, int amount)
    {
        AddItemInternal(item, amount);
    }

    // ===================================================================
    //  UI UPDATE
    // ===================================================================

    public void UpdateAllUI()
    {
        for (int i = 0; i < mainInventoryUI.Length; i++)
            mainInventoryUI[i].UpdateSlot(mainInventorySlots[i]);

        for (int i = 0; i < hotbarUI.Length; i++)
            hotbarUI[i].UpdateSlot(hotbarSlots[i]);

        UpdateEquippedItem();
    }

    // ===================================================================
    //  (Остальная часть — БЕЗ ИЗМЕНЕНИЙ)
    // ===================================================================

    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SIZE) return;

        selectedHotbarIndex = index;
        selectionHighlight.position = hotbarUI[selectedHotbarIndex].transform.position;

        UpdateEquippedItem();
    }

    private Vector3 GetSafeDropPosition()
    {
        if (playerTransform == null)
            return Vector3.zero;

        return playerTransform.position + playerTransform.forward * 1.5f + playerTransform.up * 2f;
    }

    public void DropItemFromSelectedSlot() => DropItem(selectedHotbarIndex);

    public void DropItem(int index)
    {
        var slot = hotbarSlots[index];
        if (slot.ItemData == null) return;

        Vector3 pos = GetSafeDropPosition();

        GameObject obj = Instantiate(slot.ItemData.worldPrefab, pos, Quaternion.identity);
        ItemObject io = obj.GetComponent<ItemObject>();
        io.itemData = slot.ItemData;
        io.quantity = 1;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }

        slot.RemoveFromStack(1);
        if (slot.Amount <= 0) slot.ClearSlot();

        UpdateAllUI();
    }

    public bool HasIngredients(RecipeIngredient[] ingredients)
    {
        foreach (var ing in ingredients)
        {
            int total = 0;

            foreach (var s in hotbarSlots)
                if (s.ItemData != null && s.ItemData.id == ing.item.id)
                    total += s.Amount;

            foreach (var s in mainInventorySlots)
                if (s.ItemData != null && s.ItemData.id == ing.item.id)
                    total += s.Amount;

            if (total < ing.amount)
                return false;
        }

        return true;
    }

    public void ConsumeIngredients(RecipeIngredient[] ingredients)
    {
        foreach (var ing in ingredients)
        {
            int left = ing.amount;

            // Hotbar
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                var slot = hotbarSlots[i];
                if (slot.ItemData != null && slot.ItemData.id == ing.item.id)
                {
                    int take = Mathf.Min(left, slot.Amount);
                    slot.RemoveFromStack(take);
                    if (slot.Amount <= 0) slot.ClearSlot();

                    left -= take;
                    if (left <= 0) break;
                }
            }

            // Main
            if (left > 0)
            {
                for (int i = 0; i < mainInventorySlots.Count; i++)
                {
                    var slot = mainInventorySlots[i];
                    if (slot.ItemData != null && slot.ItemData.id == ing.item.id)
                    {
                        int take = Mathf.Min(left, slot.Amount);
                        slot.RemoveFromStack(take);
                        if (slot.Amount <= 0) slot.ClearSlot();

                        left -= take;
                        if (left <= 0) break;
                    }
                }
            }
        }

        UpdateAllUI();
    }

    public void UpdateEquippedItem()
    {
        Item selected = hotbarSlots[selectedHotbarIndex].ItemData;
        EquipmentManager.instance.EquipItem(selected);
    }

    // Drag’n’Drop — оставлено без изменений
    public void OnDragBegin(InventorySlot slot)
    {
        if (slot.ItemData == null) return;

        draggedSlot = slot;
        draggableItem.sprite = slot.ItemData.icon;
        draggableItem.gameObject.SetActive(true);
    }

    public void OnDragEnd()
    {
        draggableItem.gameObject.SetActive(false);
        draggedSlot = null;
    }

    public void OnDrop(InventorySlot dropSlot)
    {
        if (draggedSlot == null) return;

        var draggedItem = draggedSlot.ItemData;
        var draggedAmount = draggedSlot.Amount;
        var draggedAmmo = draggedSlot.ammoInMagazine;

        draggedSlot.UpdateSlotData(dropSlot.ItemData, dropSlot.Amount);
        dropSlot.UpdateSlotData(draggedItem, draggedAmount, draggedAmmo);

        UpdateAllUI();
    }

    public void UpdateDraggableItemPosition(Vector2 pos)
    {
        if (draggedSlot != null)
            draggableItem.rectTransform.position = pos;
    }

    // ======================================================================
    //  BACKWARD COMPATIBILITY — методы, которые требуют другие классы
    // ======================================================================

    // Был в DropZone
    public void HandleItemDropFromDrag()
    {
        if (draggedSlot == null || draggedSlot.ItemData == null)
            return;

        Item itemToDrop = draggedSlot.ItemData;
        int count = draggedSlot.Amount;

        Vector3 pos = GetSafeDropPosition();
        GameObject obj = Instantiate(itemToDrop.worldPrefab, pos, Quaternion.identity);

        var io = obj.GetComponent<ItemObject>();
        io.itemData = itemToDrop;
        io.quantity = count;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }

        draggedSlot.ClearSlot();
        draggedSlot = null;

        UpdateAllUI();
        UpdateEquippedItem();
    }

    public void DropFullStackFromHotbar(int hotbarIndex)
    {
        InventorySlot slotToDrop = hotbarSlots[hotbarIndex];
        if (slotToDrop.ItemData == null)
        {
            return;
        }

        Item itemToDrop = slotToDrop.ItemData;
        int quantityToDrop = slotToDrop.Amount;

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

        slotToDrop.ClearSlot();

        UpdateAllUI();
        UpdateEquippedItem();
    }


    public void DropFullStackFromSelectedSlot()
    {
        DropFullStackFromHotbar(selectedHotbarIndex);
    }

    public bool IsOpen => mainInventoryPanel.activeSelf;

    public void SetOpen(bool open)
    {
        mainInventoryPanel.SetActive(open);
        isInventoryOpen = open;

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public int GetMagazineAmmoForSlot(int index)
    {
        return hotbarSlots[index].ammoInMagazine;
    }

    public int GetAmmoCount(Ammo ammo)
    {
        if (ammo == null) return 0;
        int sum = 0;

        foreach (var s in hotbarSlots)
            if (s.ItemData != null && s.ItemData.id == ammo.id)
                sum += s.Amount;

        foreach (var s in mainInventorySlots)
            if (s.ItemData != null && s.ItemData.id == ammo.id)
                sum += s.Amount;

        return sum;
    }


    public bool ConsumeItem(Item item, int count = 1)
    {
        if (item == null) return false;

        int left = count;

        foreach (var s in hotbarSlots)
        {
            if (s.ItemData != null && s.ItemData.id == item.id)
            {
                int take = Mathf.Min(s.Amount, left);
                s.RemoveFromStack(take);
                if (s.Amount <= 0) s.ClearSlot();
                left -= take;
                if (left <= 0)
                {
                    UpdateAllUI();
                    UpdateEquippedItem();
                    return true;
                }
            }
        }

        foreach (var s in mainInventorySlots)
        {
            if (s.ItemData != null && s.ItemData.id == item.id)
            {
                int take = Mathf.Min(s.Amount, left);
                s.RemoveFromStack(take);
                if (s.Amount <= 0) s.ClearSlot();
                left -= take;
                if (left <= 0)
                {
                    UpdateAllUI();
                    UpdateEquippedItem();
                    return true;
                }
            }
        }

        return false;
    }

    public InventorySlot GetSelectedSlot()
    {
        return hotbarSlots[selectedHotbarIndex];
    }

    public bool HasItemCount(Item item, int count)
    {
        if (item == null) return false;

        int total = 0;

        foreach (var slot in hotbarSlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == item.id)
                total += slot.Amount;
        }

        foreach (var slot in mainInventorySlots)
        {
            if (slot.ItemData != null && slot.ItemData.id == item.id)
                total += slot.Amount;
        }

        return total >= count;
    }

    public IEnumerable<InventorySlot> GetAllSlots()
    {
        foreach (var s in hotbarSlots) yield return s;
        foreach (var s in mainInventorySlots) yield return s;
    }

    public InventorySlot FindFirstSlotWithItem(Item item)
    {
        if (item == null) return null;

        foreach (var s in hotbarSlots)
            if (s.ItemData != null && s.ItemData.id == item.id)
                return s;

        foreach (var s in mainInventorySlots)
            if (s.ItemData != null && s.ItemData.id == item.id)
                return s;

        return null;
    }
}
