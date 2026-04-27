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
        private int stacks;

        public void Bind(string buffId, int stacks)
        {
            this.buffId = buffId;
            this.stacks = stacks;

            var cfg = BuffRegistrySO.Instance.GetById(buffId);
            if (cfg == null)
                return;

            if (icon != null)
                icon.sprite = cfg.icon;

            if (label != null)
                label.text = stacks > 1
                    ? $"{cfg.displayName} x{stacks}"
                    : cfg.displayName;
        }

        public string BuffId => buffId;
        public int Stacks => stacks;
    }
}
