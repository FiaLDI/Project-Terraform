using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Application;
using Features.Menu.Tooltip;

namespace Features.Buffs.UI
{
    public sealed class BuffTooltipTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler
    {
        private string buffId;
        private BuffSystem buffSystem;

        public void Bind(string buffId, BuffSystem system)
        {
            this.buffId = buffId;
            this.buffSystem = system;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buffSystem == null || string.IsNullOrEmpty(buffId))
            {
                Debug.LogWarning("[BuffTooltipTrigger] Not bound");
                return;
            }

            var inst = FindInstance();
            if (inst == null)
            {
                Debug.LogWarning($"[BuffTooltipTrigger] Buff '{buffId}' not found");
                return;
            }

            TooltipController.Instance.ShowBuff(inst.Config);
            TooltipController.Instance.SetPointerPosition(eventData.position);
        }

        private BuffInstance FindInstance()
        {
            foreach (var b in buffSystem.Active)
            {
                if (b?.Config?.buffId == buffId)
                    return b;
            }
            return null;
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
