using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryInputHandler : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private EquipmentManager equipmentManager;

        // 🔥 ЗАМЕНА PlayerInteraction на PlayerInteractionController
        [SerializeField] private PlayerInteractionController interactionController;

        private InputSystem_Actions inputActions;
        private ICameraControlService cameraControl;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();

            cameraControl = CameraServiceProvider.Control;

            // === Inventory ===
            inputActions.UI.OpenInventory.performed += ctx => ToggleInventory();

            // === Hotbar ===
            inputActions.UI.EquipFirst.performed += ctx => inventoryManager.SelectHotbarSlot(0);
            inputActions.UI.EquipSecond.performed += ctx => inventoryManager.SelectHotbarSlot(1);
        }

        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        private void ToggleInventory()
        {
            if (inventoryManager == null)
                return;

            bool open = !inventoryManager.IsOpen;
            inventoryManager.SetOpen(open);

            if (open)
                OpenInventoryEffects();
            else
                CloseInventoryEffects();
        }

        private void OpenInventoryEffects()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            cameraControl?.SetInputBlocked(true);
        }

        private void CloseInventoryEffects()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            cameraControl?.SetInputBlocked(false);
        }
    }
}
