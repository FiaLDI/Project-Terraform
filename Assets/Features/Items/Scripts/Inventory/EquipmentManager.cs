using System;
using TMPro;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Core References")]
    private Camera playerCamera;
    [SerializeField] private Transform adsPoint; // точка прицеливания
    [SerializeField] private Transform handTransform; // Точка в руке, где будет появляться оружие

    private GameObject currentWeaponObject; // Ссылка на префаб в руке

    [Header("Weapon State")]
    private Weapon currentWeaponData;       // Данные SO, *если* это оружие
    private Item currentEquippedItemData; // <-- НОВОЕ: Данные SO *любого* предмета
    private IUsable currentUsable;      // <-- НОВОЕ: Ссылка на IUsable компонент предмета

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        // === Автоматический поиск камеры и точки удержания ===
        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("FirstPersonCamera");
            if (camObj != null)
                playerCamera = camObj.GetComponent<Camera>();
            else
                Debug.LogWarning("EquipmentManager: объект 'FirstPersonCamera' не найден на сцене!");
        }

        if (handTransform == null)
        {
            GameObject handObj = GameObject.Find("HandleEquipPoint");
            if (handObj != null)
                handTransform = handObj.transform;
            else
                Debug.LogWarning("EquipmentManager: объект 'HandleEquipPoint' не найден на сцене!");
        }
    }

    private void Update()
    {
        // <-- УДАЛЕНО: Вся логика нажатия Input.GetMouseButtonDown(0) и Input.GetKeyDown(KeyCode.R)
        // Эта логика теперь обрабатывается в PlayerUsageController (для инпута)
        // и в скриптах Usable_ (для стрельбы/перезарядки).
    }

    // <-- УДАЛЕНО: private Weapon GetCurrentWeaponData()
    // Этот метод больше не нужен, так как мы сохраняем currentWeaponData напрямую.

    public void EquipItem(Item itemToEquip)
    {
        // 1. Уничтожаем старый предмет в руке
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        // 2. Сбрасываем состояние
        currentWeaponObject = null;
        currentWeaponData = null;
        currentEquippedItemData = null; // <-- НОВОЕ: Сбрасываем данные
        currentUsable = null;         // <-- НОВОЕ: Сбрасываем интерфейс

        // 3. Если слот пуст или у предмета нет 3D-модели, выходим
        if (itemToEquip == null || itemToEquip.worldPrefab == null)
        {
            UpdateAmmoUI();
            return;
        }

        // 4. Создаем новый предмет и сбрасываем его позицию
        currentWeaponObject = Instantiate(itemToEquip.worldPrefab, handTransform);
        //Изменение переменной, отвечающей за возможность подбора предмета через E
        var io = currentWeaponObject.GetComponent<ItemObject>();
        if (io) io.isWorldObject = false;

        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;

        // 5. Отключаем физику для предмета в руках
        Rigidbody rb = currentWeaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Отключение физики предмета");
            rb.isKinematic = true;
        }

        // 6. Инициализируем HeldItemController, если он есть
        HeldItemController itemController = currentWeaponObject.GetComponent<HeldItemController>();
        if (itemController != null)
        {
            // Передаем все три необходимые ссылки
            itemController.Initialize(playerCamera, playerRigidbody, adsPoint);
        }

        // 7. Сохраняем данные SO и ищем интерфейс IUsable
        currentEquippedItemData = itemToEquip;                 // <-- НОВОЕ
        currentWeaponData = itemToEquip as Weapon;             // <-- (Осталось)
        currentUsable = currentWeaponObject.GetComponent<IUsable>();
        Debug.Log("[EquipmentManager] currentUsable = " + currentUsable);
        if (currentUsable != null)
        {
            currentUsable.Initialize(playerCamera);
        }
        // 8. Обновляем UI в самом конце, когда все готово
        UpdateAmmoUI();
    }

    // <-- УДАЛЕНО: private void Shoot(Weapon weaponData)
    // Эта логика теперь находится в Usable_Weapon_Hitscan.cs

    // <-- УДАЛЕНО: void Reload()
    // Эта логика также будет в Usable_Weapon_Hitscan.cs или в InventoryManager

    /// <summary>
    /// Обновляет UI с количеством патронов.
    /// </summary>
    void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        if (currentWeaponData != null)
        {
            // Пытаемся получить данные о патронах из InventoryManager
            if (InventoryManager.instance != null)
            {
                int ammoInMag = InventoryManager.instance.GetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex);
                int ammoInInventory = InventoryManager.instance.GetAmmoCount(currentWeaponData.requiredAmmoType);
                ammoText.text = $"{ammoInMag} / {ammoInInventory}";
            }
            else
            {
                ammoText.text = "- / -"; // Инвентарь еще не загружен
            }
        }
        else
        {
            ammoText.text = "";
        }
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ PlayerUsageController ---

    /// <summary>
    /// Возвращает ScriptableObject текущего экипированного предмета (Tool, Weapon, etc.).
    /// </summary>
    public Item GetCurrentEquippedItem()
    {
        return currentEquippedItemData;
    }

    /// <summary>
    /// Возвращает компонент IUsable текущего экипированного предмета (или null).
    /// </summary>
    public IUsable GetCurrentUsable()
    {
        return currentUsable;
    }
}