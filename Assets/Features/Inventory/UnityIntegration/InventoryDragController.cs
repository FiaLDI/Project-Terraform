using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Player;

public class InventoryDragController : MonoBehaviour
{
    public static InventoryDragController Instance { get; private set; }

    private InventorySlotUI draggedUI;
    private InventorySlot draggedSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =====================================================
    // DRAG
    // =====================================================

    public void BeginDrag(InventorySlotUI ui, InventorySlot slot)
    {
        if (ui == null || slot == null || slot.item == null)
            return;

        draggedUI = ui;
        draggedSlot = slot;

        if (DraggableItemUI.Instance != null)
        {
            DraggableItemUI.Instance.StartDrag(
                slot.item.itemDefinition.icon
            );
        }
        else
        {
            Debug.LogWarning("[InventoryDragController] DraggableItemUI not found");
        }
    }

    public void EndDrag()
    {
        draggedUI = null;
        draggedSlot = null;

        if (DraggableItemUI.Instance != null)
            DraggableItemUI.Instance.StopDrag();
    }

    // =====================================================
    // DROP
    // =====================================================

    public void DropOnto(InventorySlotUI targetUI, InventorySlot targetSlot)
    {
        if (draggedUI == null || draggedSlot == null)
        {
            EndDrag();
            return;
        }

        if (targetUI == null || targetSlot == null)
        {
            EndDrag();
            return;
        }

        // Drop на тот же слот
        if (draggedUI == targetUI)
        {
            EndDrag();
            return;
        }

        var inventory = LocalPlayerContext.Inventory;
        if (inventory == null)
        {
            EndDrag();
            return;
        }

        var from = draggedUI.Section;
        var to = targetUI.Section;

        // ===============================
        // BAG → HAND = EQUIP
        // ===============================
        if (from == InventorySection.Bag &&
            to == InventorySection.RightHand)
        {
            inventory.Service.EquipRightHand(
                draggedUI.Index,
                InventorySection.Bag
            );

            EndDrag();
            return;
        }

        if (from == InventorySection.Bag &&
            to == InventorySection.LeftHand)
        {
            inventory.Service.EquipLeftHand(
                draggedUI.Index,
                InventorySection.Bag
            );

            EndDrag();
            return;
        }

        // ===============================
        // HAND → BAG = UNEQUIP
        // ===============================
        if (from == InventorySection.RightHand &&
            to == InventorySection.Bag)
        {
            inventory.Service.UnequipRightHand();
            EndDrag();
            return;
        }

        if (from == InventorySection.LeftHand &&
            to == InventorySection.Bag)
        {
            inventory.Service.UnequipLeftHand();
            EndDrag();
            return;
        }

        // ===============================
        // BAG ↔ BAG or HAND ↔ HAND = SWAP
        // ===============================
        inventory.Service.MoveItem(
            draggedUI.Index,
            from,
            targetUI.Index,
            to
        );

        EndDrag();
    }

}
