using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Features.Inventory.Domain;
using Features.Inventory.UI;
using Features.Player.UnityIntegration;

namespace Features.Inventory.UnityIntegration
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        [SerializeField] private float snapRadius = 50f;
        private InventoryManager inventory;
        private GameObject player;
        private readonly List<InventorySlotUI> slots = new();
        private InventorySlotUI draggedUI;
        private InventorySlot draggedSlot;
        private InventorySlotUI highlightedSlot;
        private bool droppedOnSlot;
        public InventorySlotUI HoveredSlot { get; private set; }
        public InventorySlotUI LastInteractedSlot { get; private set; }


        // =====================================================
        // INIT
        // =====================================================
        public bool IsReady => inventory != null;

        private void Start()
        {
            // Подписываемся на появление игрока (на случай если BindPlayer еще не вызывалась)
            PlayerRegistry.SubscribeLocalPlayerReady(OnLocalPlayerReady);
            
            // Проверяем, может игрок уже готов
            if (PlayerRegistry.Instance != null && PlayerRegistry.Instance.LocalPlayer != null)
            {
                OnLocalPlayerReady(PlayerRegistry.Instance);
            }
        }

        private void OnDestroy()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnLocalPlayerReady);
        }

        private void OnLocalPlayerReady(PlayerRegistry registry)
        {
            if (registry.LocalPlayer != null)
            {
                BindPlayer(registry.LocalPlayer);
                RegisterSlots(GetComponentsInChildren<InventorySlotUI>(true));
                Debug.Log("[InventoryDragController] Auto-bound to local player", this);
            }
        }

        public void BindPlayer(GameObject playerObj)
        {
            this.player = playerObj;
            inventory = playerObj.GetComponent<InventoryManager>();


            if (inventory == null)
            {
                Debug.LogError(
                    "[InventoryDragController] InventoryManager not found on player",
                    playerObj);
            }
            else
            {
                Debug.Log("[InventoryDragController] BindPlayer succeeded", this);
            }
        }

        public void RegisterSlots(IEnumerable<InventorySlotUI> uiSlots)
        {
            slots.Clear();
            slots.AddRange(uiSlots);
            Debug.Log($"[InventoryDragController] Registered {slots.Count} slots", this);
        }

        // =====================================================
        // DRAG
        // =====================================================

        public void BeginDrag(
            InventorySlotUI ui,
            InventorySlot slot,
            PointerEventData eventData)
        {
            if (!IsReady)
            {
                if (PlayerRegistry.Instance?.LocalPlayer != null)
                {
                    BindPlayer(PlayerRegistry.Instance.LocalPlayer);
                    if (slots.Count == 0)
                    {
                        RegisterSlots(GetComponentsInChildren<InventorySlotUI>(true));
                    }
                }
            }
            
            if (!IsReady || ui == null || slot == null)
                return;

            if (slot.item == null || slot.item.IsEmpty)
                return;

            draggedUI = ui;
            draggedSlot = slot;
            droppedOnSlot = false;

            DraggableItemUI.Instance
                ?.StartDrag(slot.item.itemDefinition.icon, eventData);
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

            if (!droppedOnSlot && !TrySnapToSlot(eventData))
                RequestDropToWorld();

            ClearDrag();
        }

        public void NotifyDropTarget(InventorySlotUI targetUI)
        {
            if (draggedUI == null || targetUI == null)
                return;

            droppedOnSlot = true;
            SendMoveCommand(draggedUI, targetUI);
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
            var closest = FindClosestSlot(eventData);

            if (highlightedSlot == closest)
                return;

            highlightedSlot?.SetHighlight(false);
            highlightedSlot = closest;
            highlightedSlot?.SetHighlight(true);
        }

        private bool TrySnapToSlot(PointerEventData eventData)
        {
            var target = FindClosestSlot(eventData);
            if (target == null || target == draggedUI)
                return false;

            droppedOnSlot = true;
            SendMoveCommand(draggedUI, target);
            return true;
        }

        public void SetHovered(InventorySlotUI slot)
        {
            HoveredSlot = slot;
        }

        public void ClearHovered(InventorySlotUI slot)
        {
            if (HoveredSlot == slot)
                HoveredSlot = null;
        }

        public void SetLastInteracted(InventorySlotUI slot)
        {
            LastInteractedSlot = slot;
        }

        private InventorySlotUI FindClosestSlot(PointerEventData eventData)
        {
            InventorySlotUI best = null;
            float bestDist = snapRadius;

            foreach (var slot in slots)
            {
                if (!slot.isActiveAndEnabled)
                    continue;

                if (draggedSlot?.item?.itemDefinition?.isTwoHanded == true &&
                    slot.Section == InventorySection.LeftHand)
                    continue;

                if (slot.transform is not RectTransform rt)
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

            net.RequestInventoryCommand(new InventoryCommandData
            {
                Command = InventoryCommand.MoveItem,
                FromSection = fromUI.Section,
                FromIndex = NormalizeIndex(fromUI),
                ToSection = toUI.Section,
                ToIndex = NormalizeIndex(toUI)
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
    }
}
