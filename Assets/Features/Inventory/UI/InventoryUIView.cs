using UnityEngine;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Input;
using Features.Player.UI;

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
        }

        private void OnEnable()
        {
            var root = PlayerUIRoot.I;
            if (root == null)
                return;

            if (root.BoundPlayer != null)
                OnPlayerBound(root.BoundPlayer);

            root.OnPlayerBound += OnPlayerBound;
        }

        private void OnDisable()
        {
            if (PlayerUIRoot.I != null)
                PlayerUIRoot.I.OnPlayerBound -= OnPlayerBound;
        }

        // ======================================================
        // PLAYER BIND
        // ======================================================

        private void OnPlayerBound(GameObject player)
        {
            if (player == null)
            {
                initialized = false;
                inventory = null;
                bagWindow.SetActive(false);
                return;
            }

            if (initialized)
                return;

            Debug.Log("[InventoryUIView] Bound to player: " + player.name, this);

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogError(
                    "[InventoryUIView] InventoryManager not found on player",
                    player
                );
                return;
            }

            InitDrag(player);

            inventory.OnInventoryChanged += Refresh;
            Debug.Log("[InventoryUIView] Subscribed to OnInventoryChanged");
            initialized = true;
            Refresh();

            if (pendingShow)
            {
                pendingShow = false;
                Show();
            }
        }

        private void InitDrag(GameObject player)
        {
            var drag = GetComponent<InventoryDragController>();
            if (drag == null)
            {
                Debug.LogError("[InventoryUIView] InventoryDragController not found", this);
                return;
            }

            drag.BindPlayer(player);
            drag.RegisterSlots(GetComponentsInChildren<InventorySlotUI>(true));

            foreach (var slot in GetComponentsInChildren<InventorySlotUI>(true))
            {
                slot.SetDragController(drag);
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
                bagSlots[i].Bind(model.main[i], InventorySection.Bag, i);
            }

            // ==== HOTBAR ====
            for (int i = 0; i < hotbarSlots.Length && i < model.hotbar.Count; i++)
            {
                hotbarSlots[i].Bind(model.hotbar[i], InventorySection.Hotbar, i);
            }

            // ==== HANDS ====
            leftHandSlot.Bind(model.leftHand, InventorySection.LeftHand, 0);
            rightHandSlot.Bind(model.rightHand, InventorySection.RightHand, 0);

            // ==== Hotbar Selection ====
            if (hotbarSelection != null && hotbarSlots.Length > 0)
            {
                int idx = model.selectedHotbarIndex;
                if (idx >= 0 && idx < hotbarSlots.Length)
                    hotbarSelection.position = hotbarSlots[idx].transform.position;
            }
        }
    }
}
