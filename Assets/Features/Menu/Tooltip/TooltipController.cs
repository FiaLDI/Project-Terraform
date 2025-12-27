using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Items.Domain;
using Features.Abilities.Domain;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Menu.Tooltip
{
    public sealed class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance;

        private object currentOwner;

        [Header("UI")]
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text stats;

        private RectTransform rect;
        private Canvas canvas;
        private bool isVisible;

        // последняя позиция указателя в экранных координатах
        private Vector2? lastPointerPosition;

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

            if (lastPointerPosition == null)
                return;

            UpdatePosition(lastPointerPosition.Value);
        }

        // =====================================================
        // POINTER POSITION
        // =====================================================

        public void SetPointerPosition(Vector2 screenPos)
        {
            lastPointerPosition = screenPos;
        }

        private void UpdatePosition(Vector2 screenPos)
        {
            if (canvas == null || rect == null)
                return;

            var canvasRect = canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera,
                out Vector2 localPos
            );

            float width  = rect.rect.width;
            float height = rect.rect.height;

            Vector2 offset    = new Vector2(20f, height * 0.5f + 20f);
            Vector2 targetPos = localPos + offset;

            Vector2 canvasSize = canvasRect.rect.size;
            float halfW = width  * 0.5f;
            float halfH = height * 0.5f;

            targetPos.x = Mathf.Clamp(
                targetPos.x,
                -canvasSize.x / 2f + halfW + 10f,
                canvasSize.x  / 2f - halfW - 10f
            );

            targetPos.y = Mathf.Clamp(
                targetPos.y,
                -canvasSize.y / 2f + halfH + 10f,
                canvasSize.y  / 2f - halfH - 10f
            );

            rect.anchoredPosition = targetPos;
        }

        // =====================================================
        // ITEM TOOLTIP
        // =====================================================

        public void ShowForItemInstance(ItemInstance inst, object owner)
        {
            if (inst == null || inst.itemDefinition == null)
            {
                Hide();
                return;
            }
            Debug.Log($"[Tooltip] Show item={inst.itemDefinition.id} lvl={inst.level} qty={inst.quantity}");

            currentOwner = owner;

            var def = inst.itemDefinition;
            icon.sprite      = def.icon;
            title.text       = def.itemName;
            description.text = def.description;
            stats.text       = "";

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

            currentOwner = ability;

            icon.sprite      = ability.icon;
            title.text       = ability.displayName;
            description.text = ability.description;
            stats.text       = "";

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

            currentOwner = buff;

            var cfg = buff.Config;

            icon.sprite      = cfg.icon;
            title.text       = cfg.displayName;
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
            group.alpha = 1f;
            group.blocksRaycasts = false;
        }

        public void Hide(bool instant = false)
        {
            isVisible = false;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            currentOwner = null;
        }
    }
}
