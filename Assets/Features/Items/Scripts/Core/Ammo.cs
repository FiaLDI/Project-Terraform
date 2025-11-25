using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo", menuName = "Inventory/Items/Ammo")]
public class Ammo : Item
{
    // В будущем здесь можно добавить специфичные для патронов поля,
    // например, тип урона (огненный, ледяной) или эффект при попадании.
    // Пока что он просто существует как отдельный тип предмета.
}