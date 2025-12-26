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
        private InventoryUIView inventoryUI;


        private InputAction openInventory;
        private InputAction closeInventory;
        private InputAction cancel;
        private InputAction drop;


        private bool bound;


        // ======================================================
        // INPUT BIND
        // ======================================================


        public void BindInput(PlayerInputContext ctx)
        {
            Debug.Log($"[InventoryInputHandler] BindInput called with ctx={ctx}, current={input}", this);


            if (input == ctx)
            {
                Debug.Log("[InventoryInputHandler] BindInput ignored: same context");
                return;
            }


            if (input != null)
            {
                Debug.Log("[InventoryInputHandler] Rebinding, unbinding old context");
                UnbindInput(input);
            }


            input = ctx;
            if (input == null)
            {
                Debug.LogWarning("[InventoryInputHandler] BindInput: ctx is NULL, abort");
                return;
            }


            inventoryUI = Object.FindAnyObjectByType<InventoryUIView>(
                FindObjectsInactive.Include);


            if (inventoryUI == null)
            {
                Debug.LogError("[InventoryInputHandler] InventoryUIView not found");
                return;
            }


            Debug.Log("[InventoryInputHandler] InventoryUIView found, binding actions");
            BindActions();
            bound = true;
        }


        public void UnbindInput(PlayerInputContext ctx)
        {
            Debug.Log($"[InventoryInputHandler] UnbindInput called with ctx={ctx}, current={input}, bound={bound}", this);


            if (!bound || input != ctx)
                return;


            UnbindActions();
            input = null;
            bound = false;


            Debug.Log("[InventoryInputHandler] Unbound");
        }


        // ======================================================
        // ACTIONS
        // ======================================================


        private void BindActions()
        {
            if (input == null)
            {
                Debug.LogError("[InventoryInputHandler] BindActions called with null input");
                return;
            }


            var player = input.Actions.Player;
            var ui = input.Actions.UI;


            Debug.Log("[InventoryInputHandler] Enabling actions: OpenInventory, Drop, CloseInventory, Cancel");


            Enable(player, "OpenInventory", "Drop");
            Enable(ui, "CloseInventory", "Cancel");


            openInventory = player.FindAction("OpenInventory", true);
            openInventory.started += OnOpenInventory;


            closeInventory = ui.FindAction("CloseInventory", true);
            cancel = ui.FindAction("Cancel", true);


            closeInventory.started += OnCloseInventory;
            cancel.started += OnCloseInventory;


            drop = player.FindAction("Drop", true);
            drop.performed += _ => DropFromHands();


            Debug.Log("[InventoryInputHandler] Actions bound successfully");
        }


        private void UnbindActions()
        {
            if (input == null)
                return;


            Debug.Log("[InventoryInputHandler] Unbinding actions");


            if (openInventory != null)
                openInventory.started -= OnOpenInventory;


            if (closeInventory != null)
                closeInventory.started -= OnCloseInventory;


            if (cancel != null)
                cancel.started -= OnCloseInventory;
        }


        // ======================================================
        // INVENTORY UI
        // ======================================================


        private void OnOpenInventory(InputAction.CallbackContext _)
        {
            Debug.Log($"[InventoryInputHandler] OnOpenInventory, mode={InputModeManager.I?.CurrentMode}");


            if (InputModeManager.I.CurrentMode == InputMode.Gameplay)
            {
                Debug.Log("[InventoryInputHandler] Opening inventory UI");
                inventoryUI.Open();
            }
            else
            {
                Debug.Log("[InventoryInputHandler] Ignored OpenInventory: mode != Gameplay");
            }
        }


        private void OnCloseInventory(InputAction.CallbackContext _)
        {
            Debug.Log($"[InventoryInputHandler] OnCloseInventory, mode={InputModeManager.I?.CurrentMode}");


            if (InputModeManager.I.CurrentMode == InputMode.Inventory &&
                ReferenceEquals(UIStackManager.I.Peek(), inventoryUI))
            {
                Debug.Log("[InventoryInputHandler] Closing inventory via UIStack");
                UIStackManager.I.Pop();
            }
        }


        // ======================================================
        // DROP
        // ======================================================


        private void DropFromHands()
        {
            Debug.Log("[InventoryInputHandler] DropFromHands called");


            var net = GetNet();
            if (net == null)
            {
                Debug.LogWarning("[InventoryInputHandler] DropFromHands: net is NULL");
                return;
            }


            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[InventoryInputHandler] DropFromHands: Camera.main is NULL");
                return;
            }


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
            if (player == null)
            {
                Debug.LogWarning("[InventoryInputHandler] GetNet: LocalPlayer is NULL");
                return null;
            }


            var net = player.GetComponent<InventoryStateNetwork>();
            if (net == null)
                Debug.LogWarning("[InventoryInputHandler] GetNet: InventoryStateNetwork not found on player");


            return net;
        }


        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
            {
                var a = map.FindAction(n, true);
                a.Enable();
                Debug.Log($"[InventoryInputHandler] Enabled action {map.name}/{n}");
            }
        }
    }
}