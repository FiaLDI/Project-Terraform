using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Menu.Tooltip;

public class InventorySlotUI : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    public static bool IsDragging = false;


    private InventorySlot assignedInventorySlot;

    // -----------------------------
    //   UPDATE SLOT
    // -----------------------------
    public void UpdateSlot(InventorySlot slot)
    {
        assignedInventorySlot = slot;

        if (slot.ItemData != null && slot.Amount > 0)
        {
            itemIcon.sprite = slot.ItemData.icon;
            itemIcon.enabled = true;

            if (slot.ItemData.isStackable && slot.Amount > 1)
                amountText.text = slot.Amount.ToString();
            else
                amountText.text = "";
        }
        else
        {
            ClearSlotUI();
        }
    }

    public void ClearSlotUI()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        amountText.text = "";
    }

    // -----------------------------
    //   TOOLTIP
    // -----------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDragging) return;

        if (assignedInventorySlot?.ItemData != null)
            TooltipController.Instance.ShowForItem(assignedInventorySlot.ItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsDragging) return;

        TooltipController.Instance.Hide();
    }

    // -----------------------------
    //   DRAG & DROP
    // -----------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;
        if (assignedInventorySlot.ItemData != null)
        {
            TooltipController.Instance.Hide(); // скрываем tooltip
            InventoryManager.instance.OnDragBegin(assignedInventorySlot);
            itemIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        InventoryManager.instance.UpdateDraggableItemPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        InventoryManager.instance.OnDragEnd();
        itemIcon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryManager.instance.OnDrop(assignedInventorySlot);
    }
}
