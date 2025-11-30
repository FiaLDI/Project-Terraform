using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Application;
using Features.Menu.Tooltip;

namespace Features.Buffs.UI
{
    public class BuffTooltipTrigger : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        private BuffInstance bound;

        public void Bind(BuffInstance buff)
        {
            bound = buff;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (bound != null)
                TooltipController.Instance?.ShowBuff(bound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance?.Hide();
        }
    }
}
