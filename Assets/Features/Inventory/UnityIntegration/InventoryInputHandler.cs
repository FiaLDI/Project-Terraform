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

            input.Player.Drop.performed += _ => DropItem();

            // === Scroll hotbar ===
            input.UI.ScrollWheel.performed += ctx =>
            {
                float scroll = ctx.ReadValue<Vector2>().y;
                if (scroll > 0) CycleHotbar(+1);
                else if (scroll < 0) CycleHotbar(-1);
            };

            input.UI.Cancel.performed += _ =>
            {
                if (inventoryUI.IsOpen)
                    ToggleInventory();
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

        public void CloseInventory()
        {
            if (inventoryUI.IsOpen == false)
                return;
            ToggleInventory();
        }

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

        private void DropItem()
        {
            if (inventory == null)
                return;

            var dropped = inventory.Service.DropFromHands();
            if (dropped == null)
                return;

            SpawnDroppedItem(dropped);
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
            Quaternion rot = Quaternion.identity;

            var obj = Instantiate(prefab, pos, rot);

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
