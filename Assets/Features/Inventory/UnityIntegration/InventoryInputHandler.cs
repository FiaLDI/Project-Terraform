using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;
using Features.Inventory;
using Features.Inventory.UI;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryUIView inventoryUI;
        [SerializeField] private PlayerInteractionController interactionController;

        private InputSystem_Actions input;
        private ICameraControlService cameraControl;
        private IInventoryContext inventory;

        private void Awake()
        {
            input = new InputSystem_Actions();
            input.Enable();

            cameraControl = CameraServiceProvider.Control;

            // === Inventory ===
            input.UI.OpenInventory.performed += _ => ToggleInventory();

            // === Hotbar direct ===
            input.UI.EquipFirst.performed += _ => SelectHotbar(0);
            input.UI.EquipSecond.performed += _ => SelectHotbar(1);

            input.Player.Drop.performed += _ => DropCurrent();

            // === Scroll hotbar ===
            input.UI.ScrollWheel.performed += ctx =>
            {
                float scroll = ctx.ReadValue<Vector2>().y;
                if (scroll > 0) CycleHotbar(+1);
                else if (scroll < 0) CycleHotbar(-1);
            };
        }

        private void Start()
        {
            inventory = LocalPlayerContext.Inventory;
        }

        private void OnEnable() => input.Enable();
        private void OnDisable() => input.Disable();

        // ========================================================
        // HOTBAR
        // ========================================================

        private void SelectHotbar(int index)
        {
            if (inventory == null)
                return;

            inventory.Service.SelectHotbarIndex(index);
        }

        private void CycleHotbar(int direction)
        {
            if (inventory == null)
                return;

            int max = inventory.Model.hotbar.Count;
            int current = inventory.Model.selectedHotbarIndex;

            int next = (current + direction + max) % max;
            SelectHotbar(next);
        }


        // ========================================================
        // INVENTORY UI
        // ========================================================

        private void ToggleInventory()
        {
            if (inventoryUI == null)
                return;

            bool open = !inventoryUI.IsOpen;
            inventoryUI.SetOpen(open);

            if (open) ApplyInventoryOpenEffects();
            else ApplyInventoryCloseEffects();
        }

        private void ApplyInventoryOpenEffects()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            cameraControl?.SetInputBlocked(true);
            interactionController?.SetInteractionBlocked(true);
        }

        private void ApplyInventoryCloseEffects()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            cameraControl?.SetInputBlocked(false);
            interactionController?.SetInteractionBlocked(false);
        }

        private void DropCurrent()
        {
            if (inventory == null)
                return;

            int index = inventory.Model.selectedHotbarIndex;
            var slot = inventory.Model.hotbar[index];

            if (slot.item == null)
                return;

            var inst = slot.item;

            // удалить из инвентаря
            inventory.Service.TryRemove(inst.itemDefinition, 1);

            // заспавнить в мире
            var prefab = inst.itemDefinition.worldPrefab;
            if (prefab == null)
                return;

            var worldObj = Instantiate(
                prefab,
                transform.position + transform.forward * 1.5f,
                Quaternion.identity
            );

            var holder = worldObj.GetComponent<ItemRuntimeHolder>()
                        ?? worldObj.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);

            if (worldObj.TryGetComponent<IItemModeSwitch>(out var mode))
                mode.SetWorldMode();
        }

    }
}
