using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Application;
using Features.Menu.Tooltip;

namespace Features.Buffs.UI
{
    public class BuffTooltipTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private BuffInstance inst;

        public void Bind(BuffInstance inst)
        {
            this.inst = inst;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("HOVER BUFF ICON", this);
            if (inst != null)
                TooltipController.Instance?.ShowBuff(inst);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance?.Hide();
        }
    }
}
