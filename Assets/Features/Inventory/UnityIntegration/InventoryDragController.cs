using Features.Inventory.Domain;
using Features.Inventory.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        public static InventoryDragController Instance { get; private set; }

        [SerializeField] private float snapRadius = 50f;

        private InventoryManager inventory;
        private GameObject player;

        private InventorySlotUI draggedUI;
        private InventorySlot draggedSlot;
        private InventorySlotUI highlightedSlot;

        private bool droppedOnSlot;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // вызывается из PlayerUIRoot.Bind()
        public void BindPlayer(GameObject player)
        {
            this.player = player;
            inventory = player.GetComponent<InventoryManager>();

            if (inventory == null)
            {
                Debug.LogError(
                    "[InventoryDragController] InventoryManager not found on player",
                    player);
            }
        }

        // =====================================================
        // DRAG
        // =====================================================

        public void BeginDrag(
            InventorySlotUI ui,
            InventorySlot slot,
            PointerEventData eventData)
        {
            if (inventory == null || ui == null || slot == null)
                return;

            if (slot.item == null || slot.item.IsEmpty)
                return;

            draggedUI = ui;
            draggedSlot = slot;

            DraggableItemUI.Instance
                ?.StartDrag(slot.item.itemDefinition.icon, eventData);

            droppedOnSlot = false;
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (draggedUI == null)
                return;

            UpdateHighlight(eventData);
            DraggableItemUI.Instance?.UpdateDrag(eventData);
        }

        public void EndDrag(PointerEventData eventData)
        {
            if (draggedUI == null)
            {
                ClearDrag();
                return;
            }

            if (droppedOnSlot)
            {
                ClearDrag();
                return;
            }

            if (!TrySnapToSlot(eventData))
                RequestDropToWorld();

            ClearDrag();
        }

        private void ClearDrag()
        {
            highlightedSlot?.SetHighlight(false);
            highlightedSlot = null;

            draggedUI = null;
            draggedSlot = null;

            DraggableItemUI.Instance?.StopDrag();
        }

        // =====================================================
        // SNAP / HIGHLIGHT
        // =====================================================

        private void UpdateHighlight(PointerEventData eventData)
        {
            InventorySlotUI closest = FindClosestSlot(eventData);

            if (highlightedSlot == closest)
                return;

            highlightedSlot?.SetHighlight(false);
            highlightedSlot = closest;
            highlightedSlot?.SetHighlight(true);
        }

        private bool TrySnapToSlot(PointerEventData eventData)
        {
            var targetUI = FindClosestSlot(eventData);
            if (targetUI == null || targetUI == draggedUI)
                return false;

            droppedOnSlot = true;
            SendMoveCommand(draggedUI, targetUI);
            return true;
        }

        private InventorySlotUI FindClosestSlot(PointerEventData eventData)
        {
            var slots = FindObjectsByType<InventorySlotUI>(
                FindObjectsSortMode.None);

            InventorySlotUI best = null;
            float bestDist = snapRadius;

            foreach (var slot in slots)
            {
                if (slot.transform is not RectTransform rt)
                    continue;

                if (draggedSlot?.item?.itemDefinition?.isTwoHanded == true &&
                    slot.Section == InventorySection.LeftHand)
                    continue;

                Vector2 pos = RectTransformUtility.WorldToScreenPoint(
                    eventData.pressEventCamera,
                    rt.position);

                float dist = Vector2.Distance(pos, eventData.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = slot;
                }
            }

            return best;
        }

        // =====================================================
        // COMMANDS
        // =====================================================

        private void SendMoveCommand(
            InventorySlotUI fromUI,
            InventorySlotUI toUI)
        {
            var net = GetNet();
            if (net == null)
                return;

            int fromIdx = NormalizeIndex(fromUI);
            int toIdx = NormalizeIndex(toUI);

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.MoveItem,
                FromSection = fromUI.Section,
                FromIndex = fromIdx,
                ToSection = toUI.Section,
                ToIndex = toIdx
            });
        }

        private void RequestDropToWorld()
        {
            var net = GetNet();
            if (net == null || draggedUI == null)
                return;

            var cam = UnityEngine.Camera.main;
            if (cam == null)
                return;

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.DropFromSlot,
                Section = draggedUI.Section,
                Index = NormalizeIndex(draggedUI),
                Amount = int.MaxValue,
                WorldPos = cam.transform.position,
                WorldForward = cam.transform.forward
            });
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private InventoryStateNetwork GetNet()
        {
            return player != null
                ? player.GetComponent<InventoryStateNetwork>()
                : null;
        }

        private static int NormalizeIndex(InventorySlotUI ui)
        {
            return ui.Section switch
            {
                InventorySection.LeftHand or InventorySection.RightHand => 0,
                _ => ui.Index
            };
        }

        public void NotifyDropTarget(InventorySlotUI targetUI)
        {
            if (draggedUI == null || targetUI == null)
                return;

            droppedOnSlot = true;
            SendMoveCommand(draggedUI, targetUI);
            ClearDrag();
        }
    }
}
