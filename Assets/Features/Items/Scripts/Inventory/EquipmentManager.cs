using TMPro;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;
    [SerializeField] private Rigidbody playerRigidbody;
    // Камера, использующаяся для стрельбы
    [Header("Shooting")]
    //[SerializeField] 
    private Camera playerCamera;
    [SerializeField] private Transform adsPoint; // точка прицеливания
    //[SerializeField] 
    private Transform handTransform; // Точка в руке, где будет появляться оружие
    private GameObject currentWeaponObject;
    [Header("Weapon State")]
    private Weapon currentWeaponData;
    private int currentAmmoInMag;

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
        // Если в руках нет оружия (или экипированный предмет - не оружие), то выходим.
        if (currentWeaponData == null)
        {
            return;
        }

        //// Обработка стрельбы по нажатию Левой Кнопки Мыши.
        //if (Input.GetMouseButtonDown(0))
        //{
        //    // Вызываем метод Shoot, который теперь сам проверяет наличие патронов в магазине.
        //    Shoot(currentWeaponData);
        //}

        //// Обработка перезарядки по нажатию клавиши R.
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Reload();
        //}
    }

    // Вспомогательный метод для получения данных об оружии
    private Weapon GetCurrentWeaponData()
    {
        if (currentWeaponObject == null) return null;

        // Предполагаем, что на префабе оружия висит ItemObject
        ItemObject itemObj = currentWeaponObject.GetComponent<ItemObject>();
        if (itemObj != null)
        {
            // Пробуем преобразовать Item в Weapon
            return itemObj.itemData as Weapon;
        }
        return null;
    }
    public void EquipItem(Item itemToEquip)
    {
        // 1. Уничтожаем старый предмет в руке
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        // 2. Сбрасываем состояние
        currentWeaponData = null;
        //currentWeaponAnimator = null;

        // 3. Если слот пуст или у предмета нет 3D-модели, выходим
        if (itemToEquip == null || itemToEquip.worldPrefab == null)
        {
            UpdateAmmoUI();
            return;
        }

        // 4. Создаем новый предмет и сбрасываем его позицию
        currentWeaponObject = Instantiate(itemToEquip.worldPrefab, handTransform);
        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;

        // 5. Отключаем физику для предмета в руках
        Rigidbody rb = currentWeaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 6. Инициализируем HeldItemController, если он есть
        HeldItemController itemController = currentWeaponObject.GetComponent<HeldItemController>();
        if (itemController != null)
        {
            // Передаем все три необходимые ссылки
            itemController.Initialize(playerCamera, playerRigidbody, adsPoint);
        }

        // 7. Проверяем, является ли предмет оружием, и настраиваем его
        currentWeaponData = itemToEquip as Weapon;

        // 8. Обновляем UI в самом конце, когда все готово
        UpdateAmmoUI();
    }
    private void Shoot(Weapon weaponData)
    {
        // Получаем текущее количество патронов из слота
        int ammoInMag = InventoryManager.instance.GetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex);

        if (ammoInMag <= 0)
        {
            Debug.Log("Click! Magazine is empty.");
            return;
        }

        // Уменьшаем количество и сохраняем новое значение обратно в слот
        ammoInMag--;
        InventoryManager.instance.SetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex, ammoInMag);

        UpdateAmmoUI();
        Debug.Log("Произведен выстрел.");

        // Создаем луч из камеры вперед
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hitInfo;
        float weaponRange = 100f; // Максимальная дальность стрельбы

        // Пускаем луч
        if (Physics.Raycast(ray, out hitInfo, weaponRange))
        {
            // Если луч во что-то попал, пытаемся получить у этого объекта компонент IDamageable
            IDamageable damageableTarget = hitInfo.collider.GetComponent<IDamageable>();

            // Проверяем, удалось ли его найти
            if (damageableTarget != null)
            {
                // Если да - наносим урон
                damageableTarget.TakeDamage(weaponData.damage);
            }
            else
            {
                // Если у объекта нет IDamageable, можно, например, создать эффект попадания в стену
                Debug.Log("Попадание в объект без IDamageable: " + hitInfo.collider.name);
            }
        }
    }
    void Reload()
    {
        int ammoInMag = InventoryManager.instance.GetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex);

        if (ammoInMag == currentWeaponData.magazineSize) return;

        int ammoNeeded = currentWeaponData.magazineSize - ammoInMag;
        int ammoAvailable = InventoryManager.instance.GetAmmoCount(currentWeaponData.requiredAmmoType);
        int ammoToReload = Mathf.Min(ammoNeeded, ammoAvailable);

        if (ammoToReload > 0)
        {
            Debug.Log("Reloading...");
            InventoryManager.instance.ConsumeAmmo(currentWeaponData.requiredAmmoType, ammoToReload);

            // Устанавливаем новое, пополненное значение патронов в слоте
            InventoryManager.instance.SetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex, ammoInMag + ammoToReload);

            UpdateAmmoUI();
        }
    }
    void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        if (currentWeaponData != null)
        {
            int ammoInMag = InventoryManager.instance.GetMagazineAmmoForSlot(InventoryManager.instance.selectedHotbarIndex);
            int ammoInInventory = InventoryManager.instance.GetAmmoCount(currentWeaponData.requiredAmmoType);
            ammoText.text = $"{ammoInMag} / {ammoInInventory}";
        }
        else
        {
            ammoText.text = "";
        }
    }
}