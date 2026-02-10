using Features.Buffs.Data;
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

        private string buffId;

        public void Bind(string buffId)
        {
            this.buffId = buffId;

            var cfg = BuffRegistrySO.Instance.GetById(buffId);
            if (cfg == null)
                return;

            if (icon != null)
                icon.sprite = cfg.icon;

            if (label != null)
                label.text = cfg.displayName;
        }

        public string BuffId => buffId;
    }
}
