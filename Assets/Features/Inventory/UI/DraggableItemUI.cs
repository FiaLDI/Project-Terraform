using UnityEngine;
using UnityEngine.UI;

namespace Features.Inventory.UI
{
    public class DraggableItemUI : MonoBehaviour
    {
        public static DraggableItemUI Instance;

        [SerializeField] private Image icon;

        private bool dragging;

        private void Awake()
        {
            Instance = this;
            icon.enabled = false;
        }

        public void StartDrag(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.enabled = true;
            dragging = true;
        }

        public void StopDrag()
        {
            dragging = false;
            icon.enabled = false;
        }

        private void Update()
        {
            if (dragging)
            {
                icon.transform.position = Input.mousePosition;
            }
        }
    }
}
