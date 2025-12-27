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
        [SerializeField] private GameObject leftHandWindow;
        [SerializeField] private GameObject rightHandWindow;

        [Header("Slots")]
        [SerializeField] private InventorySlotUI[] bagSlots;
        [SerializeField] private InventorySlotUI leftHandSlot;
        [SerializeField] private InventorySlotUI rightHandSlot;

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
                inventory = null;
                if (bagWindow != null)
                    bagWindow.SetActive(false);
                return;
            }

            if (initialized)
                return;

            Debug.Log("[InventoryUIView] Bound to player: " + player.name, this);

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogError("[InventoryUIView] InventoryManager not found on player", player);
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

            if (inventoryInput != null)
                inventoryInput.enabled = true;

            InputModeManager.I.SetMode(InputMode.Inventory);
        }

        public void Hide()
        {
            if (bagWindow != null)
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

            for (int i = 0; i < bagSlots.Length && i < model.main.Count; i++)
                bagSlots[i].Bind(model.main[i], InventorySection.Bag, i);

            leftHandSlot.Bind(model.leftHand, InventorySection.LeftHand, 0);
            rightHandSlot.Bind(model.rightHand, InventorySection.RightHand, 0);
        }
    }
}
