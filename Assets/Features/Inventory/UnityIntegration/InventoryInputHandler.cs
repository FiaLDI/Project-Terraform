using UnityEngine;
using UnityEngine.InputSystem;
using Features.Inventory;
using Features.Inventory.UI;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryUIView inventoryUI;

        private PlayerInputContext input;
        private IInventoryContext inventory;

        private bool subscribed;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void OnEnable()
        {
            if (input == null)
                input = GetComponentInParent<PlayerInputContext>();

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryInputHandler)}: PlayerInputContext not found",
                    this);
                return;
            }

            inventory ??= LocalPlayerContext.Inventory;

            var ui = input.Actions.UI;

            // === Inventory ===
            ui.OpenInventory.performed += OnOpenInventory;

            // === Hotbar ===
            ui.EquipFirst.performed  += OnEquipFirst;
            ui.EquipSecond.performed += OnEquipSecond;
            ui.ScrollWheel.performed += OnScroll;

            // === Drop from hands ===
            input.Actions.Player.Drop.performed += OnDrop;

            subscribed = true;
        }

        private void OnDisable()
        {
            if (!subscribed || input == null)
                return;

            var ui = input.Actions.UI;

            ui.OpenInventory.performed -= OnOpenInventory;

            ui.EquipFirst.performed  -= OnEquipFirst;
            ui.EquipSecond.performed -= OnEquipSecond;
            ui.ScrollWheel.performed -= OnScroll;

            input.Actions.Player.Drop.performed -= OnDrop;

            subscribed = false;
        }

        // ======================================================
        // INPUT HANDLERS
        // ======================================================

        private void OnOpenInventory(InputAction.CallbackContext _)
        {
            if (UIStackManager.I.HasScreens)
            {
                // если сверху инвентарь — закрываем
                if (UIStackManager.I.Peek() == inventoryUI)
                    UIStackManager.I.Pop();

                return;
            }

            // если UI пуст — открываем инвентарь
            inventoryUI.Open();
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
            DropFromHands();
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

            int count = inventory.Model.hotbar.Count;
            if (count <= 0)
                return;

            int current = inventory.Model.selectedHotbarIndex;
            int next = (current + direction + count) % count;

            SelectHotbar(next);
        }

        // ======================================================
        // DROP
        // ======================================================

        private void DropFromHands()
        {
            inventory ??= LocalPlayerContext.Inventory;
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
