using Features.Inventory.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Features.Inventory.UnityIntegration
{
    public class InventoryUIInputController : MonoBehaviour
    {
        private InputSystem_Actions input;

        private void Awake()
        {
            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Inventory.Enable();

            input.Inventory.DropOne.performed += OnDropOne;
            input.Inventory.DropAll.performed += OnDropAll;
        }

        private void OnDisable()
        {
            input.Inventory.DropOne.performed -= OnDropOne;
            input.Inventory.DropAll.performed -= OnDropAll;

            input.Inventory.Disable();
        }

        private void OnDropOne(InputAction.CallbackContext ctx)
        {
            TryDrop(false);
        }

        private void OnDropAll(InputAction.CallbackContext ctx)
        {
            TryDrop(true);
        }

        private void TryDrop(bool dropAll)
        {
            InventorySlotUI slotUI = InventorySlotUI.HoveredSlot;

            if (slotUI == null)
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
