using Features.Buffs.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Buffs.UI
{
    public sealed class BuffIconUI : MonoBehaviour
    {
        [Header("UI")]
        public Image icon;
        public TextMeshProUGUI label;

        private BuffSO cfg;

        public void Bind(BuffSO cfg)
        {
            this.cfg = cfg;

            if (cfg == null)
                return;

            if (icon != null)
                icon.sprite = cfg.icon;

            if (label != null)
                label.text = cfg.displayName;
        }

        private void OnDestroy()
        {
            cfg = null;
        }
    }
}
