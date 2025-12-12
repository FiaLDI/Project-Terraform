using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;

namespace Features.Inventory.UI
{
    /// <summary>
    /// UI-элемент ячейки инвентаря. 
    /// Показывает иконку/количество и прокидывает drag-n-drop события.
    /// НЕ содержит логики инвентаря.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;

        // Секция инвентаря (Hotbar, Bag, LeftHand, RightHand)
        public InventorySection Section { get; private set; }

        // Индекс внутри секции (0..N)
        public int Index { get; private set; }

        // Привязанный слот модели
        private InventorySlot boundSlot;

        // ===========================================================
        // BINDING
        // ===========================================================

        /// <summary>
        /// Привязать слот данных к UI.
        /// UI ничего не меняет в данных, только читает.
        /// </summary>
        public void Bind(InventorySlot slot, InventorySection section, int index)
        {
            boundSlot = slot;
            Section = section;
            Index = index;

            Refresh();
        }

        /// <summary>
        /// Обновить отображение.
        /// </summary>
        private void Refresh()
        {
            if (boundSlot == null || boundSlot.item == null)
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

        // ===========================================================
        // DRAG & DROP EVENTS
        // ===========================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boundSlot == null || boundSlot.item == null)
                return;

            InventoryDragController.Instance.BeginDrag(this, boundSlot);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // визуальное перемещение иконки делает DraggableItemUI
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InventoryDragController.Instance.EndDrag();
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventoryDragController.Instance.DropOnto(this, boundSlot);
        }
    }
}
