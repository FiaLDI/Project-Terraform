using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Inventory.UnityIntegration;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDragController : MonoBehaviour
{
    [SerializeField] private float dropSnapRadius = 50f;

    public static InventoryDragController Instance { get; private set; }

    private InventoryManager inventory;
    private GameObject player;

    private InventorySlotUI draggedUI;
    private InventorySlot draggedSlot;
    private InventorySlotUI highlightedSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Вызывается через BroadcastMessage("BindPlayer", player) из PlayerUIRoot.Bind()
    public void BindPlayer(GameObject player)
    {
        this.player = player;

        inventory = player.GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("[InventoryDragController] InventoryManager not found on player", player);
            return;
        }

        Debug.Log("[InventoryDragController] Bound to player", player);
    }

    // =====================================================
    // DRAG
    // =====================================================

    public void BeginDrag(InventorySlotUI ui, InventorySlot slot, PointerEventData eventData)
    {
        if (inventory == null || ui == null || slot?.item == null)
            return;

        draggedUI = ui;
        draggedSlot = slot;

        DraggableItemUI.Instance?.StartDrag(slot.item.itemDefinition.icon, eventData);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (inventory == null || draggedUI == null || draggedSlot == null)
            return;

        UpdateHighlight(eventData);
        DraggableItemUI.Instance?.UpdateDrag(eventData);
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (inventory == null)
        {
            ClearDrag();
            return;
        }

        if (draggedUI != null && draggedSlot != null)
        {
            bool snapped = TrySnapDrop(eventData);
            if (!snapped)
                DropDraggedToWorld();
        }

        ClearDrag();
    }

    private void ClearDrag()
    {
        if (highlightedSlot != null)
        {
            highlightedSlot.SetHighlight(false);
            highlightedSlot = null;
        }

        draggedUI = null;
        draggedSlot = null;

        DraggableItemUI.Instance?.StopDrag();
    }

    // =====================================================
    // HIGHLIGHT + SNAP
    // =====================================================

    private void UpdateHighlight(PointerEventData eventData)
    {
        var slots = Object.FindObjectsByType<InventorySlotUI>(FindObjectsSortMode.None);

        InventorySlotUI closest = null;
        float bestDist = dropSnapRadius;

        foreach (var slot in slots)
        {
            if (draggedSlot?.item?.itemDefinition?.isTwoHanded == true &&
                slot.Section == InventorySection.LeftHand)
                continue;

            if (slot.transform is not RectTransform rt)
                continue;

            Vector2 pos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rt.position);
            float dist = Vector2.Distance(pos, eventData.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                closest = slot;
            }
        }

        if (highlightedSlot == closest)
            return;

        highlightedSlot?.SetHighlight(false);
        highlightedSlot = closest;
        highlightedSlot?.SetHighlight(true);
    }

    private bool TrySnapDrop(PointerEventData eventData)
    {
        var slots = Object.FindObjectsByType<InventorySlotUI>(FindObjectsSortMode.None);

        InventorySlotUI closest = null;
        float bestDist = dropSnapRadius;

        foreach (var slot in slots)
        {
            if (draggedSlot?.item?.itemDefinition?.isTwoHanded == true &&
                slot.Section == InventorySection.LeftHand)
                continue;

            if (slot.transform is not RectTransform rt)
                continue;

            Vector2 pos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rt.position);
            float dist = Vector2.Distance(pos, eventData.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                closest = slot;
            }
        }

        if (closest == null)
            return false;

        DropOnto(closest, GetBoundSlot(closest));
        return true;
    }

    // =====================================================
    // DROP TO WORLD
    // =====================================================

    private void DropDraggedToWorld()
    {
        if (inventory == null || draggedSlot == null)
            return;

        ItemInstance inst;

        if (draggedUI.Section is InventorySection.RightHand or InventorySection.LeftHand)
        {
            inst = inventory.Service.DropFromHands();
        }
        else
        {
            inst = draggedSlot.item;
            draggedSlot.item = null;
            inventory.Service.NotifyChanged();
        }

        if (inst != null)
            SpawnWorldItem(inst);
    }

    private void SpawnWorldItem(ItemInstance inst)
    {
        if (player == null || inst?.itemDefinition?.worldPrefab == null)
            return;

        Vector3 pos =
            player.transform.position +
            player.transform.forward * 1.2f +
            Vector3.up * 0.5f;

        var obj = Instantiate(inst.itemDefinition.worldPrefab, pos, Quaternion.identity);

        var holder = obj.GetComponent<ItemRuntimeHolder>() ?? obj.AddComponent<ItemRuntimeHolder>();
        holder.SetInstance(inst);

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private InventorySlot GetBoundSlot(InventorySlotUI ui)
    {
        if (inventory == null)
            return null;

        return ui.Section switch
        {
            InventorySection.Bag => inventory.Model.main[ui.Index],
            InventorySection.Hotbar => inventory.Model.hotbar[ui.Index],
            InventorySection.LeftHand => inventory.Model.leftHand,
            InventorySection.RightHand => inventory.Model.rightHand,
            _ => null
        };
    }

    // =====================================================
    // DROP ON SLOT
    // =====================================================

    public void DropOnto(InventorySlotUI targetUI, InventorySlot targetSlot)
    {
        if (inventory == null ||
            draggedUI == null || draggedSlot == null ||
            targetUI == null || targetSlot == null ||
            draggedUI == targetUI)
        {
            ClearDrag();
            return;
        }

        var from = draggedUI.Section;
        var to = targetUI.Section;

        if (from == InventorySection.Bag && to == InventorySection.RightHand)
        {
            inventory.Service.EquipRightHand(draggedUI.Index, InventorySection.Bag);
            ClearDrag();
            return;
        }

        if (from == InventorySection.Bag && to == InventorySection.LeftHand)
        {
            inventory.Service.EquipLeftHand(draggedUI.Index, InventorySection.Bag);
            ClearDrag();
            return;
        }

        if (to == InventorySection.LeftHand &&
            draggedSlot.item?.itemDefinition?.isTwoHanded == true)
        {
            ClearDrag();
            return;
        }

        if (from == InventorySection.RightHand && to == InventorySection.Bag)
        {
            inventory.Service.MoveItem(0, InventorySection.RightHand, targetUI.Index, InventorySection.Bag);
            ClearDrag();
            return;
        }

        if (from == InventorySection.LeftHand && to == InventorySection.Bag)
        {
            inventory.Service.MoveItem(0, InventorySection.LeftHand, targetUI.Index, InventorySection.Bag);
            ClearDrag();
            return;
        }

        inventory.Service.MoveItem(draggedUI.Index, from, targetUI.Index, to);
        ClearDrag();
    }

    public void DropFromSlotToWorld(InventorySlotUI slotUI, bool dropAll)
    {
        Debug.Log("DROP: inventory = " + inventory);
        if (inventory == null || slotUI == null)
            return;

        var slot = slotUI.BoundSlot;
        if (slot?.item == null)
            return;

        ItemInstance inst;

        // Если это руки — дропаем из рук
        if (slotUI.Section is InventorySection.RightHand or InventorySection.LeftHand)
        {
            inst = inventory.Service.DropFromHands();
            if (inst == null)
                return;
        }
        else
        {
            // Bag / Hotbar — учитываем стаки
            if (dropAll || !slot.item.IsStackable || slot.item.quantity <= 1)
            {
                inst = slot.item;
                slot.item = null;
            }
            else
            {
                inst = slot.item.CloneWithQuantity(1);
                slot.item.quantity -= 1;
            }

            inventory.Service.NotifyChanged();
        }

        SpawnWorldItem(inst);
    }
}
