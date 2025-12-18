using Features.Items.Data;
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [SerializeField] private Item testWeapon; // ассет пистолета
    [SerializeField] private Item testAmmo;   // ассет патронов

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F1))
    //    {
    //        if (testWeapon != null) InventoryManager.instance.AddItem(testWeapon);
    //        Debug.Log("Пистолет добавлен");
    //    }

    //    if (Input.GetKeyDown(KeyCode.F2))
    //    {
    //        if (testAmmo != null) InventoryManager.instance.AddItem(testAmmo);
    //        Debug.Log("Патрон добавлен");
    //    }
    //}
}