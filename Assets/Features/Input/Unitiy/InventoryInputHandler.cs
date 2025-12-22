using Features.Input;
using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private IInventoryContext inventory;
        private InventoryUIView inventoryUI;

        private InputAction openInventory;
        private InputAction closeInventory;
        private InputAction cancel;
        private InputAction equipFirst;
        private InputAction equipSecond;
        private InputAction scrollWheel;
        private InputAction drop;

        private bool bound;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (input != null)
                UnbindInput(input);

            input = ctx;
            if (input == null)
                return;

            inventoryUI = Object.FindAnyObjectByType<InventoryUIView>(
                FindObjectsInactive.Include);

            if (inventoryUI == null)
            {
                Debug.LogError("[InventoryInputHandler] InventoryUIView not found");
                return;
            }

            BindActions();
            bound = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx)
                return;

            UnbindActions();
            input = null;
            inventory = null;
            bound = false;
        }

        // ======================================================
        // ACTIONS
        // ======================================================

        private void BindActions()
        {
            var player = input.Actions.Player;
            var ui = input.Actions.UI;

            Enable(player, "OpenInventory", "Drop");
            Enable(ui, "CloseInventory", "Cancel", "EquipFirst", "EquipSecond", "ScrollWheel");

            openInventory = player.FindAction("OpenInventory", true);
            openInventory.started += OnOpenInventory;

            closeInventory = ui.FindAction("CloseInventory", true);
            cancel = ui.FindAction("Cancel", true);

            closeInventory.started += OnCloseInventory;
            cancel.started += OnCloseInventory;

            equipFirst = ui.FindAction("EquipFirst", true);
            equipSecond = ui.FindAction("EquipSecond", true);
            scrollWheel = ui.FindAction("ScrollWheel", true);

            equipFirst.performed += _ => SelectHotbar(0);
            equipSecond.performed += _ => SelectHotbar(1);
            scrollWheel.performed += OnScroll;

            drop = player.FindAction("Drop", true);
            drop.performed += _ => DropFromHands();
        }

        private void UnbindActions()
        {
            if (input == null)
                return;

            openInventory.started -= OnOpenInventory;
            closeInventory.started -= OnCloseInventory;
            cancel.started -= OnCloseInventory;
            scrollWheel.performed -= OnScroll;

            equipFirst.performed -= _ => SelectHotbar(0);
            equipSecond.performed -= _ => SelectHotbar(1);
            drop.performed -= _ => DropFromHands();
        }

        // ======================================================
        // INVENTORY UI
        // ======================================================

        private void OnOpenInventory(InputAction.CallbackContext _)
        {
            if (InputModeManager.I.CurrentMode == InputMode.Gameplay)
                inventoryUI.Open();
        }

        private void OnCloseInventory(InputAction.CallbackContext _)
        {
            if (InputModeManager.I.CurrentMode == InputMode.Inventory &&
                ReferenceEquals(UIStackManager.I.Peek(), inventoryUI))
            {
                UIStackManager.I.Pop();
            }
        }

        // ======================================================
        // HOTBAR
        // ======================================================

        private void OnScroll(InputAction.CallbackContext ctx)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            if (scroll > 0) CycleHotbar(+1);
            else if (scroll < 0) CycleHotbar(-1);
        }

        private IInventoryContext Inventory =>
            inventory ??= LocalPlayerContext.Inventory;

        private void SelectHotbar(int index)
        {
            var net = GetNet();
            if (net == null)
                return;

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.SelectHotbar,
                Index = index
            });
        }

        private void CycleHotbar(int dir)
        {
            if (Inventory == null)
                return;

            int count = Inventory.Model.hotbar.Count;
            if (count == 0)
                return;

            int current = Inventory.Model.selectedHotbarIndex;
            int next = (current + dir + count) % count;

            SelectHotbar(next);
        }

        // ======================================================
        // DROP
        // ======================================================

        private void DropFromHands()
        {
            var net = GetNet();
            if (net == null)
                return;

            var cam = UnityEngine.Camera.main;
            if (cam == null)
                return;

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.DropFromSlot,
                Section = InventorySection.RightHand,
                Index = 0,
                Amount = int.MaxValue,
                WorldPos = cam.transform.position,
                WorldForward = cam.transform.forward
            });
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private InventoryStateNetwork GetNet()
        {
            var player = LocalPlayerContext.Player;
            return player != null
                ? player.GetComponent<InventoryStateNetwork>()
                : null;
        }

        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
                map.FindAction(n, true).Enable();
        }
    }
}
