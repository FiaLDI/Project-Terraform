using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Buffs.Application;
using Features.Buffs.Domain;

namespace Features.Menu.Tooltip
{
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance;

        [Header("UI")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text stats;

        private RectTransform rect;
        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            Hide(true);
        }

        private void Update()
        {
            if (group.alpha <= 0) return;
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 localPos;

            RectTransform canvasRect = canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPos
            );

            float height = rect.rect.height;
            Vector2 offset = new Vector2(0, height * 0.5f + 50);
            rect.anchoredPosition = localPos + offset;
        }

        // ============================================================
        // ITEM TOOLTIP
        // ============================================================
        public void ShowForItem(Item item)
        {
            icon.sprite = item.icon;
            title.text = item.itemName;
            description.text = item.description;

            stats.text = "";
            var cur = item.currentLevel;

            if (cur > 0)
            {
                stats.text += $"\n<color=#FFD700>Current Upgrade ({cur})</color>\n";
                var up = item.upgrades[cur - 1];

                foreach (var b in up.bonusStats)
                    stats.text += $"{b.stat}: +{b.value}\n";
            }

            group.alpha = 1;
        }

        // ============================================================
        // ABILITY TOOLTIP
        // ============================================================
        public void ShowAbility(AbilitySO ability, AbilityCaster caster)
        {
            icon.sprite = ability.icon;
            title.text = ability.displayName;
            description.text = ability.description;

            stats.text = "";

            float baseCost = ability.energyCost;
            float finalCost = baseCost;

            if (caster != null && caster.energy != null)
                finalCost = caster.energy.GetActualCost(baseCost);

            if (Mathf.Approximately(baseCost, finalCost))
                stats.text += $"<b>Energy:</b> {finalCost:0}\n";
            else
                stats.text += $"<b>Energy:</b> {finalCost:0}  <color=#888>(was {baseCost:0})</color>\n";

            stats.text += $"<b>Cooldown:</b> {ability.cooldown:0.0}s\n";

            if (ability.castType == AbilityCastType.Channel)
                stats.text += $"<b>Channel Time:</b> {ability.castTime:0.0}s\n";

            var fields = ability.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance
            );

            foreach (var f in fields)
            {
                if (f.Name is "displayName" or "description" or "icon" or
                    "cooldown" or "energyCost" or "castTime" or "castType")
                    continue;

                var value = f.GetValue(ability);

                if (value is float fVal)
                    stats.text += $"{FormatFieldName(f.Name)}: {fVal:0.##}\n";
                else if (value is int iVal)
                    stats.text += $"{FormatFieldName(f.Name)}: {iVal}\n";
            }

            group.alpha = 1;
        }

        private string FormatFieldName(string raw)
        {
            System.Text.StringBuilder sb = new();

            foreach (char c in raw)
            {
                if (char.IsUpper(c))
                    sb.Append(' ');

                sb.Append(c);
            }

            return sb.ToString().Trim().Replace(" Hp", " HP");
        }

        // ============================================================
        // BUFF TOOLTIP
        // ============================================================
        public void ShowBuff(BuffInstance buff)
        {
            var cfg = buff.Config;

            icon.sprite = cfg.icon;
            title.text = cfg.displayName;
            title.color = cfg.isDebuff ? Color.red : Color.white;

            description.text = cfg.ToString();
            stats.text = "";

            stats.text += $"<b>Effect:</b> {cfg.stat} ({cfg.modType} {cfg.value})\n";

            if (cfg.isStackable && buff.StackCount > 1)
                stats.text += $"Stacks: {buff.StackCount}\n";

            if (!float.IsInfinity(cfg.duration))
                stats.text += $"Duration: {cfg.duration:0.0}s\n";
            else
                stats.text += $"Duration: <i>Permanent</i>\n";

            if (cfg is GlobalBuffSO g)
                stats.text += $"<color=#88F>GLOBAL Buff</color>\nKey: {g.key}\n";

            if (cfg.name.ToLower().Contains("aura"))
                stats.text += "<color=#8F8>Aura Effect</color>\n";

            group.alpha = 1;
        }

        public void Hide(bool instant = false)
        {
            group.alpha = 0;
        }
    }
}
