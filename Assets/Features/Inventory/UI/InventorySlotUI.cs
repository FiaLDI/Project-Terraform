using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Menu.Tooltip;

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

        public InventorySlot BoundSlot => boundSlot;

        public InventorySection Section { get; private set; }
        public int Index { get; private set; }

        private InventorySlot boundSlot;

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
            if (boundSlot?.item == null)
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
        }

        public void SetHighlight(bool value)
        {
            if (highlight != null)
                highlight.SetActive(value);
        }

        // ===========================================================
        // DRAG & DROP
        // ===========================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boundSlot?.item == null)
                return;

            TooltipController.Instance?.Hide();

            InventoryDragController.Instance.BeginDrag(
                this,
                boundSlot,
                eventData
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance.EndDrag(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryDragController.Instance.DropOnto(this, boundSlot);
        }

        // ===========================================================
        // TOOLTIP / HOVER
        // ===========================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundSlot?.item == null)
                return;

            HoveredSlot = this;
            TooltipController.Instance?.ShowForItemInstance(boundSlot.item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoveredSlot == this)
                HoveredSlot = null;

            TooltipController.Instance?.Hide();
        }
    }
}
