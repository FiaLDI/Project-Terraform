using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Inventory.Domain;
using Features.Menu.Tooltip;
using Features.Items.Domain;
using Features.Inventory.UnityIntegration;

namespace Features.Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private GameObject highlight;

        public static InventorySlotUI HoveredSlot { get; private set; }
        public static InventorySlotUI LastInteractedSlot { get; private set; }

        public InventorySlot BoundSlot => boundSlot;
        public InventorySection Section { get; private set; }
        public int Index { get; private set; }

        private InventorySlot boundSlot;
        private InventoryDragController dragController;

        // ===========================================================
        // BINDING
        // ===========================================================

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

                if (HoveredSlot == this)
                {
                    HoveredSlot = null;
                    TooltipController.Instance?.Hide();
                }

                return;
            }

            icon.enabled = true;
            icon.sprite = boundSlot.item.itemDefinition.icon;

            amountText.text =
                boundSlot.item.quantity > 1
                    ? boundSlot.item.quantity.ToString()
                    : "";
        }



        public void SetHighlight(bool value)
        {
            if (highlight != null)
                highlight.SetActive(value);
        }

        // ===========================================================
        // DRAG & DROP
        // ===========================================================

        private InventoryDragController Drag
        {
            get
            {
                if (dragController == null)
                    dragController =
                        GetComponentInParent<InventoryDragController>(true);

                return dragController;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boundSlot == null ||
                boundSlot.item == null ||
                boundSlot.item.IsEmpty)
                return;

            LastInteractedSlot = this;
            TooltipController.Instance?.Hide();

            InventoryDragController.Instance
                ?.BeginDrag(this, boundSlot, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance?.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance?.EndDrag(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryDragController.Instance?.NotifyDropTarget(this);
        }


        private void OnDisable()
        {
            if (HoveredSlot == this)
                HoveredSlot = null;

            if (LastInteractedSlot == this)
                LastInteractedSlot = null;

            TooltipController.Instance?.Hide();
        }

        // ===========================================================
        // TOOLTIP / HOVER
        // ===========================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundSlot == null ||
                boundSlot.item == null ||
                boundSlot.item.IsEmpty)
                return;

            HoveredSlot = this;
            LastInteractedSlot = this;

            TooltipController.Instance
                ?.ShowForItemInstance(boundSlot.item, this);
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoveredSlot == this)
                HoveredSlot = null;

            TooltipController.Instance?.Hide();
        }
    }
}
