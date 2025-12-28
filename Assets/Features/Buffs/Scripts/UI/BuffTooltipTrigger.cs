using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Application;
using Features.Menu.Tooltip;

namespace Features.Buffs.UI
{
    public class BuffTooltipTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler  // 🟢 ДОБАВИТЬ
    {
        private BuffInstance inst;

        public void Bind(BuffInstance inst)
        {
            this.inst = inst;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("[BuffTooltipTrigger] OnPointerEnter", this);

            if (inst == null)
            {
                Debug.LogWarning("[BuffTooltipTrigger] Buff instance is null", this);
                return;
            }

            if (TooltipController.Instance == null)
            {
                Debug.LogError("[BuffTooltipTrigger] TooltipController.Instance is null!", this);
                return;
            }

            // 🟢 Показать tooltip
            TooltipController.Instance.ShowBuff(inst);

            // 🟢 КРИТИЧНО: передать позицию указателя
            if (eventData != null)
            {
                TooltipController.Instance.SetPointerPosition(eventData.position);
                Debug.Log($"[BuffTooltipTrigger] Set tooltip position: {eventData.position}", this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("[BuffTooltipTrigger] OnPointerExit", this);
            TooltipController.Instance?.Hide();
        }

        // 🟢 ДОБАВИТЬ: обновлять позицию при движении мыши
        public void OnPointerMove(PointerEventData eventData)
        {
            if (TooltipController.Instance != null && inst != null)
            {
                TooltipController.Instance.SetPointerPosition(eventData.position);
            }
        }
    }
}
