using Features.Inventory.Domain;
using Features.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryUIInputController :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool subscribed;

        private InventoryDragController drag;
        private InputAction dropOneAction;
        private InputAction dropAllAction;
        private System.Action<InputAction.CallbackContext> dropOneHandler;
        private System.Action<InputAction.CallbackContext> dropAllHandler;

        public void SetContext(InventoryDragController dragCtrl)
        {
            Debug.LogWarning("IS DRAGFFFFF");
            drag = dragCtrl;
        }

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            Unsubscribe();
            input = ctx;
            Subscribe();
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (input != ctx)
                return;

            Unsubscribe();
            input = null;
        }

        private void Subscribe()
        {
            if (subscribed || input == null)
                return;

            var ui = input.Actions.UI;

            dropOneAction = ui.FindAction("DropOne", false);
            dropAllAction = ui.FindAction("DropAll", false);

            if (dropOneAction == null || dropAllAction == null)
            {
                UnityEngine.Debug.LogError("[InventoryUIInputController] DropOne/DropAll not found in UI map", this);
                return;
            }

            dropOneHandler = _ => TryDrop(false);
            dropAllHandler = _ => TryDrop(true);

            dropOneAction.performed += dropOneHandler;
            dropAllAction.performed += dropAllHandler;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            if (dropOneAction != null && dropOneHandler != null)
                dropOneAction.performed -= dropOneHandler;
            if (dropAllAction != null && dropAllHandler != null)
                dropAllAction.performed -= dropAllHandler;

            dropOneAction = null;
            dropAllAction = null;
            dropOneHandler = null;
            dropAllHandler = null;
            subscribed = false;
        }

        private void TryDrop(bool dropAll)
        {
            if (drag == null)
            {
                UnityEngine.Debug.LogError("NOT DRUG");
                return;
            }

            var slotUI =
                drag.HoveredSlot ??
                drag.LastInteractedSlot;

            if (slotUI == null || slotUI.BoundSlot == null || slotUI.BoundSlot.item.IsEmpty)
                return;

            var net = LocalPlayerContext.Inventory
                ?.GetComponent<InventoryStateNetwork>();

            if (net == null)
                return;

            var cam = UnityEngine.Camera.main;
            if (cam == null)
                return;

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.DropFromSlot,
                Section = slotUI.Section,
                Index = slotUI.Index,
                Amount = dropAll ? int.MaxValue : 1,
                WorldPos = cam.transform.position,
                WorldForward = cam.transform.forward
            });
        }
    }
}
