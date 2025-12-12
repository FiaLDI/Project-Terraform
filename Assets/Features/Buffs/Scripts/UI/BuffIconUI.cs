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
                return;

            icon.sprite = inst.Config.icon;
        }

        private void Update()
        {
            if (inst == null || inst.Config == null)
                return;

            // Бесконечные баффы
            if (float.IsInfinity(inst.Config.duration))
            {
                timer.text = "";
                radialFill.fillAmount = 0f;
                return;
            }

            radialFill.fillAmount = inst.Progress01;

            float t = inst.Remaining;
            timer.text = t < 1f ? $"{t:0.0}" : $"{t:0}";
        }
    }
}
