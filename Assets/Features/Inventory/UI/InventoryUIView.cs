using Features.Input;
using Features.Inventory.Domain;
using Features.Inventory.UnityIntegration;
using Features.Player.UI;
using UnityEngine;

namespace Features.Inventory.UI
{
    public class InventoryUIView : MonoBehaviour, IUIScreen
    {
        [Header("Windows")]
        [SerializeField] private GameObject bagWindow;
        [SerializeField] private GameObject activeSlotsWindow;

        [Header("Slots")]
        [SerializeField] private InventorySlotUI[] bagSlots;
        [SerializeField] private InventorySlotUI activeSlot0Slot;
        [SerializeField] private InventorySlotUI activeSlot1Slot;
        [SerializeField] private InventorySlotUI activeSlot2Slot;

        [Header("Input")]
        [SerializeField] private InventoryUIInputController inventoryInput; // ВИСИТ НА UI, НЕ НА PLAYER

        public InputMode Mode => InputMode.Inventory;

        private InventoryManager inventory;
        private bool initialized;
        private bool pendingShow;

        private void Awake()
        {
            if (bagWindow != null)
                bagWindow.SetActive(false);

            if (activeSlotsWindow != null)
                activeSlotsWindow.SetActive(true);
        }

        private void Start()
        {
            Debug.Log("[InventoryUIView] Start", this);

            var root = PlayerUIRoot.I;
            Debug.Log($"[InventoryUIView] PlayerUIRoot.I = {root}", this);

            if (root == null)
            {
                Debug.LogWarning("[InventoryUIView] PlayerUIRoot is null!", this);
                return;
            }

            Debug.Log($"[InventoryUIView] root.BoundPlayer = {root.BoundPlayer}", this);

            if (root.BoundPlayer != null)
                OnPlayerBound(root.BoundPlayer);

            root.OnPlayerBound += OnPlayerBound;
            Debug.Log("[InventoryUIView] Subscribed to OnPlayerBound");
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

                if (inventory != null)
                    inventory.OnInventoryChanged -= Refresh;

                inventory = null;

                if (bagWindow != null)
                    bagWindow.SetActive(false);
                if (activeSlotsWindow != null)
                    activeSlotsWindow.SetActive(true);

                return;
            }

            // 🔥 если уже был другой inventory — отписываемся
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogError("[InventoryUIView] InventoryManager not found on player", player);
                return;
            }

            Debug.Log("[InventoryUIView] Rebinding to player: " + player.name, this);

            InitDrag(player);

            inventory.OnInventoryChanged += Refresh;

            initialized = true;

            Refresh();
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
                slot.SetDragController(drag);

            if (inventoryInput != null)
                inventoryInput.SetContext(drag);
            else
                Debug.LogError("[InventoryUIView] inventoryInput IS NULL", this);
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

            if (bagWindow != null)
                bagWindow.SetActive(true);

            if (activeSlotsWindow != null)
                activeSlotsWindow.SetActive(true);

            if (inventoryInput != null)
                inventoryInput.enabled = true;

            InputModeManager.I.SetMode(InputMode.Inventory);
        }

        public void Hide()
        {
            if (bagWindow != null)
                bagWindow.SetActive(false);

            if (activeSlotsWindow != null)
                activeSlotsWindow.SetActive(true);

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
            
            Debug.Log("[InventoryUIView] Refresh()");

            for (int i = 0; i < bagSlots.Length && i < model.main.Count; i++)
                bagSlots[i].Bind(model.main[i], InventorySection.Bag, i);

            if (activeSlot0Slot != null)
            {
                activeSlot0Slot.Bind(model.activeSlot0, InventorySection.ActiveSlot0, 0);
                activeSlot0Slot.SetSelected(model.ActiveSlotIndex == 0);
            }

            if (activeSlot1Slot != null)
            {
                activeSlot1Slot.Bind(model.activeSlot1, InventorySection.ActiveSlot1, 0);
                activeSlot1Slot.SetSelected(model.ActiveSlotIndex == 1);
            }

            if (activeSlot2Slot != null)
            {
                activeSlot2Slot.Bind(model.activeSlot2, InventorySection.ActiveSlot2, 0);
                activeSlot2Slot.SetSelected(model.ActiveSlotIndex == 2);
            }
        }
    }
}
