using UnityEngine;
using Features.Inventory;
using Features.Inventory.Domain;
using Features.Player;
using Features.Inventory.UnityIntegration;

namespace Features.Inventory.UI
{
    /// <summary>
    /// UI слой инвентаря: отображает сумку, хотбар, руки.
    /// Не содержит логики — только отображение.
    /// </summary>
    public class InventoryUIView : MonoBehaviour
    {
        [Header("Windows")]
        [SerializeField] private GameObject bagWindow;
        [SerializeField] private GameObject leftHandWindow;
        [SerializeField] private GameObject rightHandWindow;

        [Header("Slots")]
        [SerializeField] private InventorySlotUI[] bagSlots;
        [SerializeField] private InventorySlotUI[] hotbarSlots;
        [SerializeField] private InventorySlotUI leftHandSlot;
        [SerializeField] private InventorySlotUI rightHandSlot;

        [Header("Hotbar")]
        [SerializeField] private RectTransform hotbarSelection;

        [SerializeField]
        private InventoryUIInputController inventoryInput;

        public bool IsOpen => bagWindow.activeSelf;

        private IInventoryContext inventory;


        private void Start()
        {
            inventory = LocalPlayerContext.Inventory;

            if (inventory == null)
            {
                Debug.LogError("[InventoryUIView] InventoryContext is NULL");
                enabled = false;
                return;
            }

            inventory.Service.OnChanged += Refresh;

            bagWindow.SetActive(false);

            Refresh();
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.Service.OnChanged -= Refresh;
        }

        public void SetOpen(bool open)
        {
            if (bagWindow != null)
                bagWindow.SetActive(open);

            Cursor.visible = open;
            Cursor.lockState = open
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            if (inventoryInput != null)
                inventoryInput.enabled = open;
        }

        public void Refresh()
        {
            if (inventory == null)
                return;

            var model = inventory.Model;

            // ==== BAG ====
            for (int i = 0; i < bagSlots.Length && i < model.main.Count; i++)
            {
                bagSlots[i].Bind(
                    model.main[i],
                    InventorySection.Bag,
                    i
                );
            }

            // ==== HOTBAR ====
            for (int i = 0; i < hotbarSlots.Length && i < model.hotbar.Count; i++)
            {
                hotbarSlots[i].Bind(
                    model.hotbar[i],
                    InventorySection.Hotbar,
                    i
                );
            }

            // ==== HANDS ====
            leftHandSlot.Bind(
                model.leftHand,
                InventorySection.LeftHand,
                0
            );

            rightHandSlot.Bind(
                model.rightHand,
                InventorySection.RightHand,
                0
            );

            // ==== Hotbar Selection ====
            if (hotbarSelection != null &&
                hotbarSlots != null &&
                hotbarSlots.Length > 0)
            {
                int idx = model.selectedHotbarIndex;
                if (idx >= 0 && idx < hotbarSlots.Length)
                {
                    hotbarSelection.position =
                        hotbarSlots[idx].transform.position;
                }
            }
        }
    }
}
