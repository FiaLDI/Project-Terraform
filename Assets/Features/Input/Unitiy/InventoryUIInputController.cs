using System.Collections.Generic;
using Features.Input;
using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryUIInputController : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool subscribed;

        // ======================================================
        // BIND INPUT
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            Unsubscribe();

            input = ctx;

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryUIInputController)}: BindInput with NULL",
                    this);
                return;
            }
            Subscribe();
        }

        private void OnEnable()
        {
            if (input != null)
                Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // ======================================================
        // SUBSCRIBE
        // ======================================================

        private void Subscribe()
        {
            if (subscribed || input == null)
                return;

            var ui = input.Actions.UI;

            ui.FindAction("DropOne", true).performed += OnDropOne;
            ui.FindAction("DropAll", true).performed += OnDropAll;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            var ui = input.Actions.UI;

            ui.FindAction("DropOne", true).performed -= OnDropOne;
            ui.FindAction("DropAll", true).performed -= OnDropAll;

            subscribed = false;
        }

        // ======================================================
        // INPUT
        // ======================================================

        private void OnDropOne(InputAction.CallbackContext _)
        {
            Debug.Log("DROP ONE INPUT");
            TryDrop(dropAll: false);
        }

        private void OnDropAll(InputAction.CallbackContext _)
        {
            Debug.Log("DROP ALL INPUT");
            TryDrop(dropAll: true);
        }

        // ======================================================
        // DROP LOGIC
        // ======================================================

        private void TryDrop(bool dropAll)
        {
            // 1. Берём слот
            InventorySlotUI slotUI =
                InventorySlotUI.HoveredSlot ??
                InventorySlotUI.LastInteractedSlot;

            if (slotUI == null &&
                EventSystem.current != null &&
                Mouse.current != null)
            {
                var results = new List<RaycastResult>();
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };

                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var r in results)
                {
                    slotUI = r.gameObject.GetComponentInParent<InventorySlotUI>();
                    if (slotUI != null)
                        break;
                }
            }

            if (slotUI == null)
                return;

            if (slotUI.BoundSlot == null || slotUI.BoundSlot.item == null)
                return;

            var dc = InventoryDragController.Instance;
            if (dc == null)
                return;

            dc.DropFromSlotToWorld(slotUI, dropAll);
        }
    }
}
