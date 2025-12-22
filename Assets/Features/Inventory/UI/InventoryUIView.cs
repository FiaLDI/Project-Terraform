using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Input;
using Features.Player.UnityIntegration;

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


        private InventoryManager inventory;
        private bool initialized;
        private bool pendingShow;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            if (bagWindow != null)
                bagWindow.SetActive(false);

            PlayerRegistry.OnLocalPlayerReady += OnLocalPlayerReady;
        }
        
        private void OnPlayerBound(GameObject player)
        {
            if (initialized)
                return;

            Debug.Log("[InventoryUIView] OnPlayerBound: " + player.name, this);

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogError(
                    "[InventoryUIView] InventoryManager not found on player",
                    player
                );
                return;
            }

            if (inventory.IsReady)
                OnInventoryReady();
            else
                inventory.OnReady += OnInventoryReady;
        }

        private void OnLocalPlayerReady(PlayerRegistry registry)
        {
            if (registry == null || registry.LocalPlayer == null)
                return;

            BindPlayer(registry.LocalPlayer);
        }

        public void BindPlayer(GameObject player)
        {
            OnPlayerBound(player);
        }

        private void OnInventoryReady()
        {
            if (initialized)
                return;

            inventory.OnReady -= OnInventoryReady;

            if (inventory == null || inventory.Service == null)
            {
                Debug.LogError("[InventoryUIView] Inventory not valid on ready", this);
                return;
            }

            inventory.Service.OnChanged += Refresh;

            initialized = true;
            Refresh();

            Debug.Log("[InventoryUIView] READY + refreshed", this);

            if (pendingShow)
            {
                pendingShow = false;
                Show();
            }
        }

        // ======================================================
        // IUIScreen
        // ======================================================

        public void Show()
        {
            if (!initialized)
            {
                pendingShow = true;
                return;
            }

            bagWindow.SetActive(true);

            if (inventoryInput != null)
                inventoryInput.enabled = true;

            InputModeManager.I.SetMode(InputMode.Inventory);
        }

        public void Hide()
        {
            bagWindow.SetActive(false);

            if (inventoryInput != null)
                inventoryInput.enabled = false;

            InputModeManager.I.SetMode(InputMode.Gameplay);
        }

        public void Open()
        {
            UIStackManager.I.Push(this);
        }

        private void OnDestroy()
        {
            PlayerRegistry.OnLocalPlayerReady -= OnLocalPlayerReady;
        }

        // ======================================================
        // UI REFRESH
        // ======================================================

        public void Refresh()
        {
            if (!initialized || inventory == null)
                return;

            var model = inventory.Model;
            if (model == null)
                return;

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
