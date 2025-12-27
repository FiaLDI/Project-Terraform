using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Inventory.Domain;
using Features.Menu.Tooltip;
using Features.Inventory.UnityIntegration;

namespace Features.Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private GameObject highlight;

        public InventorySlot BoundSlot => boundSlot;
        public InventorySection Section { get; private set; }
        public int Index { get; private set; }

        private InventorySlot boundSlot;
        private InventoryDragController drag;
        private bool isPointerOver;

        // =====================================================
        // BIND
        // =====================================================

        public void Bind(InventorySlot slot, InventorySection section, int index)
        {
            boundSlot = slot;
            Section = section;
            Index = index;

            Refresh();
        }

        private void Refresh()
        {
            if (icon == null || amountText == null)
                return;

            if (boundSlot == null ||
                boundSlot.item == null ||
                boundSlot.item.IsEmpty)
            {
                icon.enabled = false;
                amountText.text = "";
                return;
            }

            icon.enabled = true;
            icon.sprite = boundSlot.item.itemDefinition.icon;

            amountText.text =
                boundSlot.item.quantity > 1
                    ? boundSlot.item.quantity.ToString()
                    : "";
            
            if (isPointerOver)
                TooltipController.Instance?.ShowForItemInstance(boundSlot.item, this);
        }

        public void SetHighlight(bool value)
        {
            if (highlight != null)
                highlight.SetActive(value);
        }

        // =====================================================
        // DRAG
        // =====================================================

        private InventoryDragController Drag
        {
            get
            {
                if (drag == null)
                    drag = GetComponentInParent<InventoryDragController>(true);
                return drag;
            }
        }

        public void SetDragController(InventoryDragController controller)
        {
            drag = controller;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boundSlot == null ||
                boundSlot.item == null ||
                boundSlot.item.IsEmpty)
                return;

            if (Drag == null || !Drag.IsReady)
                return;

            TooltipController.Instance?.Hide();
            TooltipController.Instance?.SetPointerPosition(eventData.position);

            Drag.BeginDrag(this, boundSlot, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            TooltipController.Instance?.SetPointerPosition(eventData.position);
            Drag?.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Drag?.EndDrag(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Drag?.NotifyDropTarget(this);
        }

        private void OnDisable()
        {
            TooltipController.Instance?.Hide();
        }

        // =====================================================
        // TOOLTIP
        // =====================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundSlot == null || boundSlot.item == null || boundSlot.item.IsEmpty)
                return;
            
            isPointerOver = true;

            Drag?.SetHovered(this);
            Drag?.SetLastInteracted(this);

            TooltipController.Instance?.SetPointerPosition(eventData.position);
            TooltipController.Instance?.ShowForItemInstance(boundSlot.item, this);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            TooltipController.Instance?.SetPointerPosition(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            Drag?.ClearHovered(this);
            TooltipController.Instance?.Hide();
        }
    }
}
