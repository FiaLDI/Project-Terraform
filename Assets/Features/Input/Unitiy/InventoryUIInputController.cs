using Features.Inventory.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Features.Input;
using Features.Player;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryUIInputController : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool subscribed;

        // ======================================================
        // BIND INPUT (от LocalPlayerController)
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

            var inv = input.Actions.UI;

            inv.FindAction("DropOne").performed += OnDropOne;
            inv.FindAction("DropAll").performed += OnDropAll;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            var inv = input.Actions.UI;

            inv.FindAction("DropOne").performed -= OnDropOne;
            inv.FindAction("DropAll").performed -= OnDropAll;

            subscribed = false;
        }

        // ======================================================
        // INPUT
        // ======================================================

        private void OnDropOne(InputAction.CallbackContext _)
        {
            TryDrop(dropAll: false);
        }

        private void OnDropAll(InputAction.CallbackContext _)
        {
            TryDrop(dropAll: true);
        }

        // ======================================================
        // DROP LOGIC
        // ======================================================

        private void TryDrop(bool dropAll)
        {
            if (InputModeManager.I.CurrentMode != InputMode.Inventory)
                return;

            InventorySlotUI slotUI = InventorySlotUI.HoveredSlot;

            if (slotUI == null && EventSystem.current != null && Mouse.current != null)
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

            InventoryDragController.Instance
                ?.DropFromSlotToWorld(slotUI, dropAll);
        }
    }
}
