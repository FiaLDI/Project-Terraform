using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Player;

public class InventoryDragController : MonoBehaviour
{
    public static InventoryDragController Instance;

    private InventorySlotUI draggedUI;
    private InventorySlot draggedSlot;

    private void Awake()
    {
        Instance = this;
    }

    public void BeginDrag(InventorySlotUI ui, InventorySlot slot)
    {
        draggedUI = ui;
        draggedSlot = slot;

        if (slot.item != null)
            DraggableItemUI.Instance.StartDrag(slot.item.itemDefinition.icon);
    }

    public void EndDrag()
    {
        draggedUI = null;
        draggedSlot = null;
        DraggableItemUI.Instance.StopDrag();
    }

    public void DropOnto(InventorySlotUI targetUI, InventorySlot targetSlot)
    {
        if (draggedSlot == null)
            return;

        var inventory = LocalPlayerContext.Inventory;
        inventory?.Service.MoveItem(
            draggedUI.Index,
            draggedUI.Section,
            targetUI.Index,
            targetUI.Section
        );

        EndDrag();
    }
}
