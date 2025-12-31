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
        IPointerMoveHandler
    {
        private BuffInstance inst;

        public void Bind(BuffInstance inst)
        {
            this.inst = inst;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            #if UNITY_EDITOR
            Debug.Log("[BuffTooltipTrigger] OnPointerEnter", this);
            #endif

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

            TooltipController.Instance.ShowBuff(inst);

            if (eventData != null)
            {
                TooltipController.Instance.SetPointerPosition(eventData.position);
                Debug.Log($"[BuffTooltipTrigger] Set tooltip position: {eventData.position}", this);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            #if UNITY_EDITOR
            Debug.Log("[BuffTooltipTrigger] OnPointerExit", this);
            #endif
            TooltipController.Instance?.Hide();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (TooltipController.Instance != null && inst != null)
            {
                TooltipController.Instance.SetPointerPosition(eventData.position);
            }
        }
    }
}
