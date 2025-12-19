using Features.Inventory.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Features.Player;
using Features.Input;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryUIInputController : MonoBehaviour
    {
        private PlayerInputContext input;
        private bool subscribed;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void OnEnable()
        {
            // Получаем контекст ввода безопасно
            if (input == null)
                input = GetComponentInParent<PlayerInputContext>();

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(InventoryUIInputController)}: " +
                    $"{nameof(PlayerInputContext)} not found in parents",
                    this);
                return;
            }

            var inv = input.Actions.UI;

            inv.DropOne.performed += OnDropOne;
            inv.DropAll.performed += OnDropAll;

            subscribed = true;
        }

        private void OnDisable()
        {
            if (!subscribed || input == null)
                return;

            var inv = input.Actions.UI;

            inv.DropOne.performed -= OnDropOne;
            inv.DropAll.performed -= OnDropAll;

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
