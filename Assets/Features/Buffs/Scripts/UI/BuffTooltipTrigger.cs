using UnityEngine;
using UnityEngine.EventSystems;
using Features.Buffs.Domain;
using Features.Menu.Tooltip;

namespace Features.Buffs.UI
{
    public sealed class BuffTooltipTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerMoveHandler
    {
        private BuffSO cfg;

        // =====================================================
        // BIND
        // =====================================================

        public void Bind(BuffSO cfg)
        {
            this.cfg = cfg;
        }

        // =====================================================
        // POINTER
        // =====================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (cfg == null)
            {
                Debug.LogWarning("[BuffTooltipTrigger] Not bound");
                return;
            }

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

        private void OnDestroy()
        {
            cfg = null;
        }
    }
}
