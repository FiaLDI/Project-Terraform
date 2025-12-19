using UnityEngine;
using UnityEngine.InputSystem;
using Features.Inventory;
using Features.Inventory.UI;
using Features.Items.Domain;
using Features.Items.UnityIntegration;
using Features.Player;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private IInventoryContext inventory;
        private InventoryUIView inventoryUI;

        private InputAction openInventoryPlayer;
        private InputAction openInventoryUI;
        private InputAction equipFirst;
        private InputAction equipSecond;
        private InputAction scrollWheel;
        private InputAction drop;

        private bool subscribed;

        // ======================================================
        // BIND INPUT (ТОЛЬКО ПОДПИСКИ)
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (ctx == null)
            {
                Debug.LogError("[InventoryInputHandler] BindInput with NULL context", this);
                return;
            }

            if (subscribed)
                return;

            input = ctx;

            inventoryUI = FindObjectOfType<InventoryUIView>(true);
            if (inventoryUI == null)
            {
                Debug.LogError("[InventoryInputHandler] InventoryUIView not found");
                return;
            }

            var ui = input.Actions.UI;
            var player = input.Actions.Player;

            openInventoryPlayer = player.FindAction("OpenInventory", true);
            openInventoryUI     = ui.FindAction("OpenInventory", true);
            equipFirst          = ui.FindAction("EquipFirst", true);
            equipSecond         = ui.FindAction("EquipSecond", true);
            scrollWheel         = ui.FindAction("ScrollWheel", true);
            drop                = player.FindAction("Drop", true);

            openInventoryPlayer.performed += OnToggleInventory;
            openInventoryUI.performed     += OnToggleInventory;
            equipFirst.performed          += _ => SelectHotbar(0);
            equipSecond.performed         += _ => SelectHotbar(1);
            scrollWheel.performed         += OnScroll;
            drop.performed                += _ => DropFromHands();

            subscribed = true;

            Debug.Log("[InventoryInputHandler] Input bound");
        }

        private void OnDisable()
        {
            if (!subscribed)
                return;

            openInventoryPlayer.performed -= OnToggleInventory;
            openInventoryUI.performed     -= OnToggleInventory;
            scrollWheel.performed         -= OnScroll;

            subscribed = false;
        }

        // ======================================================
        // TOGGLE INVENTORY
        // ======================================================

        private void OnToggleInventory(InputAction.CallbackContext _)
        {
            if (UIStackManager.I == null)
                return;

            if (UIStackManager.I.HasScreens)
            {
                if (UIStackManager.I.Peek() == inventoryUI)
                    UIStackManager.I.Pop();

                return;
            }

            inventoryUI.Open();
        }

        // ======================================================
        // SCROLL / HOTBAR
        // ======================================================

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (scroll > 0) CycleHotbar(+1);
            else if (scroll < 0) CycleHotbar(-1);
        }

        private IInventoryContext Inventory
        {
            get
            {
                inventory ??= LocalPlayerContext.Inventory;
                return inventory;
            }
        }

        private void SelectHotbar(int index)
        {
            if (Inventory == null)
                return;

            Inventory.Service.SelectHotbarIndex(index);
        }

        private void CycleHotbar(int direction)
        {
            if (Inventory == null)
                return;

            int count = Inventory.Model.hotbar.Count;
            if (count <= 0)
                return;

            int current = Inventory.Model.selectedHotbarIndex;
            int next = (current + direction + count) % count;

            SelectHotbar(next);
        }

        // ======================================================
        // DROP
        // ======================================================

        private void DropFromHands()
        {
            if (Inventory == null)
                return;

            var dropped = Inventory.Service.DropFromHands();
            if (dropped == null)
                return;

            SpawnDroppedItem(dropped);
        }

        private void SpawnDroppedItem(ItemInstance inst)
        {
            if (inst?.itemDefinition?.worldPrefab == null)
                return;

            var player = LocalPlayerContext.Player;
            if (player == null)
                return;

            Vector3 pos =
                player.transform.position +
                player.transform.forward * 1.2f;

            var obj = Instantiate(
                inst.itemDefinition.worldPrefab,
                pos,
                Quaternion.identity);

            var holder =
                obj.GetComponent<ItemRuntimeHolder>() ??
                obj.AddComponent<ItemRuntimeHolder>();

            holder.SetInstance(inst);

            if (obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(
                    player.transform.forward * 2f,
                    ForceMode.Impulse);
            }
        }
    }
}
