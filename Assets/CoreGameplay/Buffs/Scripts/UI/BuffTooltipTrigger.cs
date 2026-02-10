using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Domain;
using Features.Menu.Tooltip;
using Features.Buffs.Data;

namespace Features.Buffs.UI
{
    public sealed class BuffTooltipTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler
    {
        private string buffId;

        public void Bind(string buffId)
        {
            this.buffId = buffId;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var cfg = BuffRegistrySO.Instance.GetById(buffId);
            if (cfg == null)
                return;

            TooltipController.Instance?.ShowBuff(cfg);
            TooltipController.Instance?.SetPointerPosition(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance?.Hide();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            TooltipController.Instance?.SetPointerPosition(eventData.position);
        }
    }
}
