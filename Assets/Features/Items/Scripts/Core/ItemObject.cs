using UnityEngine;

public class ItemObject : MonoBehaviour
{
    // Ссылка на ScriptableObject, который определяет, что это за предмет
    public Item itemData;

    // Количество этого предмета, которое добавится в инвентарь
    public int quantity = 1;

    // Этот метод вызывается в редакторе каждый раз, когда меняется значение в инспекторе.
    // Он помогает избежать ошибок при настройке.
    private void OnValidate()
    {
        // Если у нас есть данные о предмете
        if (itemData != null)
        {
            // Устанавливаем имя объекта в редакторе для удобства
            gameObject.name = itemData.name + " (" + quantity + ")";

            // Если предмет не стакается, количество всегда должно быть 1
            if (!itemData.isStackable)
            {
                quantity = 1;
            }
            // Если предмет стакается, его количество не может превышать размер стака
            else if (quantity > itemData.maxStackAmount)
            {
                quantity = itemData.maxStackAmount;
            }
            // Количество не может быть меньше 1
            if (quantity < 1)
            {
                quantity = 1;
            }
        }
    }
}