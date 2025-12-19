using UnityEngine;
using Features.Inventory.Domain;
using Features.Player;
using Features.Inventory.UnityIntegration;

namespace Features.Inventory.UI
{
    public class InventoryUIView : MonoBehaviour, IUIScreen
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

        public InputMode Mode => InputMode.Inventory;

        private IInventoryContext inventory;
        private bool initialized;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Start()
        {
            if (LocalPlayerContext.IsReady)
            {
                Init();
            }
            else
            {
                LocalPlayerContext.OnReady += Init;
            }

            bagWindow.SetActive(false);
        }

        private void OnDestroy()
        {
            LocalPlayerContext.OnReady -= Init;

            if (inventory != null)
                inventory.Service.OnChanged -= Refresh;
        }

        // ======================================================
        // INIT
        // ======================================================

        private void Init()
        {
            if (initialized)
                return;

            var inv = LocalPlayerContext.Inventory;
            if (inv == null)
                return;

            inv.OnReady += () =>
            {
                inventory = inv;
                inventory.Service.OnChanged += Refresh;
                Refresh();
                initialized = true;
            };
        }

        // ======================================================
        // IUIScreen
        // ======================================================

        public void Show()
        {
            bagWindow.SetActive(true);

            if (inventoryInput != null)
                inventoryInput.enabled = true;
        }

        public void Hide()
        {
            bagWindow.SetActive(false);

            if (inventoryInput != null)
                inventoryInput.enabled = false;
        }

        // ======================================================
        // STACK API
        // ======================================================

        public void Open()
        {
            UIStackManager.I.Push(this);
        }

        // ======================================================
        // UI REFRESH
        // ======================================================

        public void Refresh()
        {
            if (!initialized || inventory == null)
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
