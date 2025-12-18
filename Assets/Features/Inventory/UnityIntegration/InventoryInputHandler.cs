using UnityEngine;
using UnityEngine.InputSystem;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Inventory;
using Features.Inventory.UI;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;
using Features.Camera.UnityIntegration;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryUIView inventoryUI;
        [SerializeField] private PlayerInteractionController interactionController;

        private PlayerInputContext input;
        private ICameraControlService cameraControl;
        private IInventoryContext inventory;

        private bool subscribed;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void OnEnable()
        {
            // PlayerInputContext
            if (input == null)
                input = GetComponentInParent<PlayerInputContext>();

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryInputHandler)}: PlayerInputContext not found",
                    this);
                return;
            }

            // Camera service
            cameraControl ??= CameraServiceProvider.Control;

            // Inventory (может быть не готов сразу)
            inventory ??= LocalPlayerContext.Inventory;

            var ui = input.Actions.UI;
            var p  = input.Actions.Player;

            // === Inventory ===
            ui.OpenInventory.performed += OnToggleInventory;
            ui.Cancel1.performed        += OnCancel;

            // === Hotbar ===
            ui.EquipFirst.performed  += OnEquipFirst;
            ui.EquipSecond.performed += OnEquipSecond;
            ui.ScrollWheel.performed += OnScroll;

            // === Drop (из рук) ===
            p.Drop.performed += OnDrop;

            subscribed = true;
        }

        private void OnDisable()
        {
            if (!subscribed || input == null)
                return;

            var ui = input.Actions.UI;
            var p  = input.Actions.Player;

            ui.OpenInventory.performed -= OnToggleInventory;
            ui.Cancel1.performed        -= OnCancel;

            ui.EquipFirst.performed  -= OnEquipFirst;
            ui.EquipSecond.performed -= OnEquipSecond;
            ui.ScrollWheel.performed -= OnScroll;

            p.Drop.performed -= OnDrop;

            subscribed = false;
        }

        // ======================================================
        // INPUT HANDLERS
        // ======================================================

        private void OnToggleInventory(InputAction.CallbackContext _)
        {
            ToggleInventory();
        }

        private void OnCancel(InputAction.CallbackContext _)
        {
            if (inventoryUI != null && inventoryUI.IsOpen)
                ToggleInventory();
        }

        private void OnEquipFirst(InputAction.CallbackContext _) => SelectHotbar(0);
        private void OnEquipSecond(InputAction.CallbackContext _) => SelectHotbar(1);

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (scroll > 0) CycleHotbar(+1);
            else if (scroll < 0) CycleHotbar(-1);
        }

        private void OnDrop(InputAction.CallbackContext _)
        {
            DropItem();
        }

        // ======================================================
        // HOTBAR
        // ======================================================

        private void SelectHotbar(int index)
        {
            inventory ??= LocalPlayerContext.Inventory;
            if (inventory == null)
                return;

            inventory.Service.SelectHotbarIndex(index);
        }

        private void CycleHotbar(int direction)
        {
            inventory ??= LocalPlayerContext.Inventory;
            if (inventory == null)
                return;

            int max = inventory.Model.hotbar.Count;
            if (max <= 0)
                return;

            int current = inventory.Model.selectedHotbarIndex;
            int next = (current + direction + max) % max;

            SelectHotbar(next);
        }

        // ======================================================
        // INVENTORY UI
        // ======================================================

        private void ToggleInventory()
        {
            if (inventoryUI == null)
                return;

            bool open = !inventoryUI.IsOpen;
            inventoryUI.SetOpen(open);

            if (open)
                ApplyInventoryOpenEffects();
            else
                ApplyInventoryCloseEffects();
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

        // ======================================================
        // DROP
        // ======================================================

        private void DropItem()
        {
            inventory ??= LocalPlayerContext.Inventory;
            if (inventory == null)
                return;

            var dropped = inventory.Service.DropFromHands();
            if (dropped == null)
                return;

            SpawnDroppedItem(dropped);
        }

        public void CloseInventory()
        {
            if (inventoryUI == null || !inventoryUI.IsOpen)
                return;

            ToggleInventory();
        }

        private void SpawnDroppedItem(ItemInstance inst)
        {
            var prefab = inst.itemDefinition.worldPrefab;
            if (prefab == null)
                return;

            var player = LocalPlayerContext.Player;
            if (player == null)
                return;

            Vector3 pos = player.transform.position + player.transform.forward * 1.2f;
            var obj = Instantiate(prefab, pos, Quaternion.identity);

            var holder = obj.GetComponent<ItemRuntimeHolder>()
                         ?? obj.AddComponent<ItemRuntimeHolder>();
            holder.SetInstance(inst);

            if (obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(player.transform.forward * 2f, ForceMode.Impulse);
            }
        }
    }
}
