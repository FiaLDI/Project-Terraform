using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Features.Inventory.UI
{
    public class DraggableItemUI : MonoBehaviour
    {
        public static DraggableItemUI Instance;

        [SerializeField] private Image icon;
        [SerializeField] private Canvas canvas;

        private RectTransform iconRect;
        private RectTransform canvasRect;
        private CanvasGroup canvasGroup;

        private bool dragging;
        private Vector2 lastPointerPosition;

        private void Awake()
        {
            Instance = this;

            iconRect = icon.rectTransform;
            canvasRect = canvas.transform as RectTransform;

            canvasGroup = GetComponent<CanvasGroup>() 
                          ?? gameObject.AddComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            icon.raycastTarget = false;
            icon.enabled = false;
        }

        public void StartDrag(Sprite sprite, PointerEventData eventData)
        {
            if (sprite == null)
                return;

            icon.sprite = sprite;
            icon.enabled = true;
            dragging = true;

            lastPointerPosition = eventData.position;
            UpdatePosition(lastPointerPosition);
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;

            lastPointerPosition = eventData.position;
            UpdatePosition(lastPointerPosition);
        }

        public void StopDrag()
        {
            dragging = false;
            icon.enabled = false;
            icon.sprite = null;
        }

        private void UpdatePosition(Vector2 screenPos)
        {
            UnityEngine.Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPos,
                    cam,
                    out var localPos))
            {
                iconRect.anchoredPosition = localPos;
            }
        }
    }
}
