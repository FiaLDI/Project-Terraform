using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Buffs.Application;

namespace Features.Buffs.UI
{
    public class BuffIconUI : MonoBehaviour
    {
        public Image icon;
        public Image radialFill;
        public TextMeshProUGUI timer;

        private BuffInstance inst;

        public void Bind(BuffInstance inst)
        {
            this.inst = inst;

            if (inst == null || inst.Config == null)
            {
                Debug.LogError("[BuffIconUI] Invalid buff instance", this);
                return;
            }

            if (icon == null)
            {
                Debug.LogError("[BuffIconUI] icon Image is null!", this);
                return;
            }

            icon.sprite = inst.Config.icon;
            
            Debug.Log($"[BuffIconUI] Bound to buff {inst.Config.buffId}, icon sprite set", this);
        }

        private void Update()
        {
            if (inst == null || inst.Config == null)
                return;

            // Бесконечные баффы
            if (float.IsInfinity(inst.Config.duration))
            {
                if (timer != null)
                    timer.text = "";
                
                if (radialFill != null)
                    radialFill.fillAmount = 0f;
                
                return;
            }

            // Ограниченные по времени бафы
            if (radialFill != null)
                radialFill.fillAmount = inst.Progress01;

            if (timer != null)
            {
                float t = inst.Remaining;
                timer.text = t < 1f ? $"{t:0.0}" : $"{t:0}";
            }
        }
    }
}
