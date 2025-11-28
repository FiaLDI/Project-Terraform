using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item item;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipController.Instance.ShowForItem(item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipController.Instance.Hide();
    }
}
