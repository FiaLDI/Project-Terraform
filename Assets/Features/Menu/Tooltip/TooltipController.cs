using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

using Features.Abilities.Domain;
using Features.Abilities.Application;
using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;

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

            RectTransform canvasRect = canvas.transform as RectTransform;

            // Конвертация позиции в локальные координаты Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPos
            );

            // Отступ над мышкой
            float height = rect.rect.height;
            float width = rect.rect.width;

            Vector2 offset = new Vector2(20, height * 0.5f + 20);
            Vector2 targetPos = localPos + offset;

            // === ОГРАНИЧЕНИЕ ВНУТРИ CANVAS ===
            Vector2 canvasSize = canvasRect.rect.size;

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            // Левая граница
            targetPos.x = Mathf.Clamp(targetPos.x, -canvasSize.x / 2 + halfW + 10, canvasSize.x / 2 - halfW - 10);
            // Нижняя граница
            targetPos.y = Mathf.Clamp(targetPos.y, -canvasSize.y / 2 + halfH + 10, canvasSize.y / 2 - halfH - 10);

            rect.anchoredPosition = targetPos;
        }


        // =========================================================================
        // ITEM TOOLTIP
        // =========================================================================
        public void ShowForItem(Item item)
        {
            if (item == null) { Hide(); return; }

            icon.sprite = item.icon;
            title.text = item.itemName;
            title.color = Color.white;
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

        // =========================================================================
        // ABILITY TOOLTIP (адаптирована для PlayerRegistry)
        // =========================================================================
        public void ShowAbility(AbilitySO ability, AbilityCaster casterOverride = null)
        {
            if (ability == null)
            {
                Hide();
                return;
            }

            var caster = casterOverride ?? PlayerRegistry.Instance.LocalAbilities;

            icon.sprite = ability.icon;
            title.text = ability.displayName;
            title.color = Color.white;
            description.text = ability.description;
            stats.text = "";

            // === Energy Cost ===
            float baseCost = ability.energyCost;
            float finalCost = caster != null
                ? caster.GetFinalEnergyCost(ability)
                : baseCost;

            if (Mathf.Approximately(baseCost, finalCost))
                stats.text += $"<b>Energy:</b> {finalCost:0}\n";
            else
                stats.text += $"<b>Energy:</b> {finalCost:0} <color=#999>(was {baseCost:0})</color>\n";

            // === Cooldown ===
            stats.text += $"<b>Cooldown:</b> {ability.cooldown:0.0}s\n";

            // === Channel Time ===
            if (ability.castType == AbilityCastType.Channel)
                stats.text += $"<b>Channel:</b> {ability.castTime:0.0}s\n";

            // === Additional Ability Fields ===
            var fields = ability.GetType().GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
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

            return sb.ToString().Trim();
        }

        // =========================================================================
        // BUFF TOOLTIP
        // =========================================================================
        public void ShowBuff(BuffInstance buff)
        {
            if (buff == null || buff.Config == null)
            {
                Hide();
                return;
            }

            var cfg = buff.Config;

            icon.sprite = cfg.icon;
            title.text = cfg.displayName;
            title.color = cfg.isDebuff ? Color.red : Color.white;

            description.text = cfg.description;
            stats.text = "";

            stats.text += $"<b>Effect:</b> {cfg.stat} ({cfg.modType} {cfg.value})\n";

            if (cfg.isStackable && buff.StackCount > 1)
                stats.text += $"Stacks: {buff.StackCount}\n";

            if (!float.IsInfinity(cfg.duration))
                stats.text += $"Duration: {cfg.duration:0.0}s\n";
            else
                stats.text += $"Duration: <i>Permanent</i>\n";

            if (cfg.targetType == BuffTargetType.Global)
                stats.text += "<color=#88F>GLOBAL Buff</color>\n";

            group.alpha = 1;
        }

        // =========================================================================
        // HIDE TOOLTIP
        // =========================================================================
        public void Hide(bool instant = false)
        {
            group.alpha = 0;
        }
    }
}
