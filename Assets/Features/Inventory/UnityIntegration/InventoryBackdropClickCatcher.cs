using UnityEngine;
using UnityEngine.EventSystems;
using Features.Inventory.UnityIntegration;

public class InventoryBackdropClickCatcher : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryInputHandler inventoryInput;

    private void Awake()
    {
        if (inventoryInput == null)
            inventoryInput = GetComponentInParent<InventoryInputHandler>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;
        
        
    }
}
