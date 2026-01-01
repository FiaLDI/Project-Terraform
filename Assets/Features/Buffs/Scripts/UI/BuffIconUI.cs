using Features.Buffs.Application;
using Features.Buffs.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Buffs.UI
{
    public sealed class BuffIconUI : MonoBehaviour
    {
        public Image icon;
        public Image radialFill;
        public TextMeshProUGUI timer;

        private BuffInstance inst;
        private bool infinite;

        public void Bind(BuffInstance inst)
        {
            this.inst = inst;

            if (inst?.Config == null)
                return;

            infinite = inst.LifetimeMode == BuffLifetimeMode.WhileSourceAlive;

            if (icon != null)
                icon.sprite = inst.Config.icon;
        }

        private void Update()
        {
            if (inst == null || inst.Config == null)
                return;

            if (infinite)
            {
                if (radialFill != null)
                    radialFill.fillAmount = 0f;

                if (timer != null)
                    timer.text = "";

                return;
            }

            float remaining = Mathf.Max(0f, inst.Remaining);

            if (radialFill != null)
                radialFill.fillAmount = inst.Progress01;

            if (timer != null)
                timer.text = remaining < 1f ? $"{remaining:0.0}" : $"{remaining:0}";
        }


        private void OnDestroy()
        {
            inst = null;
        }
    }
}
