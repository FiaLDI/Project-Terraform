using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems; 

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    private InventorySlot assignedInventorySlot;
    // Метод для полного обновления вида слота
    public void UpdateSlot(InventorySlot slot)
    {
        Debug.Log("InventorySlotUI");
        assignedInventorySlot = slot; // Сохраняем ссылку
        if (slot.ItemData != null && slot.Amount > 0)
        {
            // Слот не пуст
            itemIcon.sprite = slot.ItemData.icon;
            itemIcon.enabled = true; // Показываем иконку

            // Показываем количество, только если предмет стакается и его больше 1
            if (slot.ItemData.isStackable && slot.Amount > 1)
            {
                amountText.text = slot.Amount.ToString();
            }
            else
            {
                amountText.text = "";
            }
        }
        else
        {
            // Слот пуст
            ClearSlotUI();
        }
    }

    // Метод для очистки отображения слота
    public void ClearSlotUI()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        amountText.text = "";
    }
    #region Drag & Drop Handlers

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedInventorySlot.ItemData != null)
        {
            // Сообщаем менеджеру, что мы начали тащить этот слот
            InventoryManager.instance.OnDragBegin(assignedInventorySlot);
            // Делаем иконку в слоте полупрозрачной для визуальной обратной связи
            itemIcon.color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Не трогаем чужой Image напрямую, а просто просим менеджера обновить позицию.
        InventoryManager.instance.UpdateDraggableItemPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Сообщаем менеджеру, что перетаскивание закончилось
        InventoryManager.instance.OnDragEnd();
        // Возвращаем иконке нормальную непрозрачность
        itemIcon.color = Color.white;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Сообщаем менеджеру, что на слот что-то "бросили"
        InventoryManager.instance.OnDrop(assignedInventorySlot);
    }

    #endregion
}