using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // Этот метод вызывается автоматически, когда игрок отпускает мышку
    // над этим UI-элементом после перетаскивания.
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Предмет выброшен в DropZone!");
        // сообщаем менеджеру, что нужно обработать выброс
        InventoryManager.instance.HandleItemDropFromDrag();
    }
}