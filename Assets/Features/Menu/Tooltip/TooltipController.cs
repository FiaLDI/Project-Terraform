using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Features.Items.Domain;
using Features.Items.Data;
using Features.Abilities.Domain;
using Features.Player.UnityIntegration;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Menu.Tooltip
{
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance;
        private Object currentOwner;

        [Header("UI")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text stats;

        private RectTransform rect;
        private Canvas canvas;

        private bool isVisible;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            Instance = this;
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            Hide(true);
        }

        private void Update()
        {
            if (!isVisible)
                return;

            if (currentOwner == null)
            {
                Hide();
                return;
            }

            if (Mouse.current == null)
                return;

            UpdatePosition();
        }

        // =====================================================
        // POSITION
        // =====================================================

        private void UpdatePosition()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RectTransform canvasRect = canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera,
                out Vector2 localPos
            );

            float width = rect.rect.width;
            float height = rect.rect.height;

            Vector2 offset = new Vector2(20, height * 0.5f + 20);
            Vector2 targetPos = localPos + offset;

            Vector2 canvasSize = canvasRect.rect.size;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            targetPos.x = Mathf.Clamp(
                targetPos.x,
                -canvasSize.x / 2 + halfW + 10,
                canvasSize.x / 2 - halfW - 10
            );

            targetPos.y = Mathf.Clamp(
                targetPos.y,
                -canvasSize.y / 2 + halfH + 10,
                canvasSize.y / 2 - halfH - 10
            );

            rect.anchoredPosition = targetPos;
        }

        // =====================================================
        // ITEM TOOLTIP
        // =====================================================

        public void ShowForItemInstance(ItemInstance inst, Object owner)
        {
            if (inst == null || inst.itemDefinition == null)
            {
                Hide();
                return;
            }

            currentOwner = owner;

            Item def = inst.itemDefinition;
            icon.sprite = def.icon;
            title.text = def.itemName;
            description.text = def.description;
            stats.text = "";

            // Level
            if (inst.level > 0 && def.upgrades != null &&
                inst.level <= def.upgrades.Length)
            {
                stats.text += $"<color=#FFD700>Level {inst.level}</color>\n";

                var up = def.upgrades[inst.level - 1];
                if (up != null)
                {
                    foreach (var b in up.bonusStats)
                        stats.text += $"{b.stat}: +{b.value}\n";
                }
            }

            // Stack
            if (def.isStackable)
                stats.text += $"\nStack: {inst.quantity}/{def.maxStackAmount}";

            Show();
        }

        // =====================================================
        // ABILITY TOOLTIP
        // =====================================================

        public void ShowAbility(AbilitySO ability)
        {
            if (ability == null)
            {
                Hide();
                return;
            }

            icon.sprite = ability.icon;
            title.text = ability.displayName;
            description.text = ability.description;
            stats.text = "";

            stats.text += $"Energy: {ability.energyCost}\n";
            stats.text += $"Cooldown: {ability.cooldown:0.0}s\n";

            Show();
        }

        // =====================================================
        // BUFF TOOLTIP
        // =====================================================

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
            description.text = cfg.description;

            stats.text =
                $"{cfg.stat} {cfg.modType} {cfg.value}\n" +
                (cfg.isDebuff ? "<color=red>Debuff</color>" : "");

            Show();
        }

        // =====================================================
        // VISIBILITY
        // =====================================================

        private void Show()
        {
            isVisible = true;
            group.alpha = 1;
            group.blocksRaycasts = false;
        }

        public void Hide(bool instant = false)
        {
            isVisible = false;
            group.alpha = 0;
            group.blocksRaycasts = false;
        }
    }
}
