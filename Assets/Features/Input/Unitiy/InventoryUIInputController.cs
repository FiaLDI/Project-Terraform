using Features.Inventory.Domain;
using Features.Inventory.UI;
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
            ui.FindAction("DropOne", true).performed += _ => TryDrop(false);
            ui.FindAction("DropAll", true).performed += _ => TryDrop(true);

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            var ui = input.Actions.UI;
            ui.FindAction("DropOne", true).performed -= _ => TryDrop(false);
            ui.FindAction("DropAll", true).performed -= _ => TryDrop(true);

            subscribed = false;
        }

        private void TryDrop(bool dropAll)
        {
            var drag = GetComponentInParent<InventoryDragController>();
            if (drag == null)
                return;

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
