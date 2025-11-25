using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] 
    private PlayerCameraController playerCameraController;
    [Header("Inventory References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EquipmentManager equipmentManager;
    [Header("Interaction References")]
    [SerializeField] 
    private PlayerInteraction playerInteraction;

    private InputSystem_Actions inputActions;
    private void Awake()
    {

        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        // === INVENTORY & ITEMS ===
        inputActions.UI.OpenInventory.performed += ctx => ToggleInventory();
        inputActions.UI.EquipFirst.performed += ctx => inventoryManager.SelectHotbarSlot(0);
        inputActions.UI.EquipSecond.performed += ctx => inventoryManager.SelectHotbarSlot(1);

        inputActions.Player.Interact.performed += ctx => playerInteraction.TryInteract();
        inputActions.Player.Drop.performed += ctx => playerInteraction.DropCurrentItem(false);
        //inputActions.Player.Use.performed += ctx => equipmentManager.TryUseCurrentItem();
        //inputActions.Player.AltUse.performed += ctx => equipmentManager.TryAltUseCurrentItem();
        //inputActions.Player.Drop.performed += ctx => inventoryManager.DropItemFromSelectedSlot();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void ToggleInventory()
    {
        if (inventoryManager == null) return;

        bool newState = !inventoryManager.IsOpen;
        inventoryManager.SetOpen(newState);

        if (newState)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerCameraController.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCameraController.enabled = true;
        }
    }
}
