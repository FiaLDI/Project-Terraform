using System;
using TMPro;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [SerializeField] private Rigidbody playerRigidbody;

    [Header("References")]
    private Camera playerCamera;
    [SerializeField] private Transform adsPoint;       
    [SerializeField] private Transform handTransform;  

    private GameObject currentItemObject;  
    private Item currentEquippedItemData; 
    private Weapon currentWeaponData;     

    private IUsable currentUsable;
    private PlayerUsageController usageController;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log($"[EquipmentManager] Awake: Instance = {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Duplicate EquipmentManager destroyed ({gameObject.name}), instance = {instance.gameObject.name}");
            Destroy(gameObject);
            return;
        }

        // ===== AUTO CAMERA DETECT =====
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogError("[EquipmentManager] Camera.main not found!");
        }

        // ===== AUTO HAND TRANSFORM =====
        if (handTransform == null)
        {
            GameObject handObj = GameObject.Find("HandleEquipPoint");
            if (handObj != null)
                handTransform = handObj.transform;
            else
                Debug.LogError("[EquipmentManager] HandleEquipPoint not found in scene!");
        }

        usageController = GetComponent<PlayerUsageController>();
        if (usageController == null)
            Debug.LogError("[EquipmentManager] PlayerUsageController missing on player!");
    }

    // ====================================================================
    // EQUIP LOGIC
    // ====================================================================
    public void EquipItem(Item itemToEquip)
    {
        // REMOVE OLD ITEM
        if (currentItemObject != null)
            Destroy(currentItemObject);

        currentItemObject = null;
        currentEquippedItemData = null;
        currentWeaponData = null;
        currentUsable = null;

        if (itemToEquip == null || itemToEquip.worldPrefab == null)
        {
            UpdateAmmoUI();
            return;
        }

        // SPAWN NEW ITEM
        currentItemObject = Instantiate(itemToEquip.worldPrefab, handTransform);
        currentItemObject.transform.localPosition = Vector3.zero;
        currentItemObject.transform.localRotation = Quaternion.identity;

        // DISABLE PHYSICS
        var rb = currentItemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // remove world pickup state
        var io = currentItemObject.GetComponent<ItemObject>();
        if (io) io.isWorldObject = false;

        // STORE SCRIPTABLE DATA
        currentEquippedItemData = itemToEquip;
        currentWeaponData = itemToEquip as Weapon;

        // FIND USABLE
        currentUsable = currentItemObject.GetComponent<IUsable>();
        Debug.Log($"[EquipmentManager] Found IUsable: {currentUsable}");

        if (currentUsable != null)
            currentUsable.Initialize(playerCamera);

        // ===== APPLY RUNTIME STATS =====
        if (InventoryManager.instance != null)
        {
            ItemRuntimeStats runtime = InventoryManager.instance.GetRuntimeStats(itemToEquip);

            var statReceiver = currentItemObject.GetComponent<IStatItem>();
            if (statReceiver != null)
            {
                Debug.Log($"[EquipmentManager] Applying runtime stats → {runtime}");
                statReceiver.ApplyRuntimeStats(runtime);
            }
            else
            {
                Debug.Log("[EquipmentManager] This item has NO IStatItem, skipping stats");
            }
        }

        // INFORM USAGE CONTROLLER
        if (usageController != null)
            usageController.OnItemEquipped(currentUsable);

        UpdateAmmoUI();
    }

    // ====================================================================
    // UI
    // ====================================================================
    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        if (currentWeaponData != null)
        {
            if (InventoryManager.instance == null)
            {
                ammoText.text = "- / -";
                return;
            }

            int ammoInMag = InventoryManager.instance.GetMagazineAmmoForSlot(
                InventoryManager.instance.selectedHotbarIndex
            );

            int ammoInInventory = InventoryManager.instance.GetAmmoCount(
                currentWeaponData.requiredAmmoType
            );

            ammoText.text = $"{ammoInMag} / {ammoInInventory}";
        }
        else
        {
            ammoText.text = "";
        }
    }

    // ====================================================================
    // PUBLIC API
    // ====================================================================
    public Item GetCurrentEquippedItem() => currentEquippedItemData;

    public IUsable GetCurrentUsable() => currentUsable;
}
