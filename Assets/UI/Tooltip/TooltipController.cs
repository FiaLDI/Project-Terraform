using Features.Abilities.Domain;
using Features.Buffs.Domain;
using Features.Items.Domain;
using Features.Stats.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            float width = rect.rect.width;
            float height = rect.rect.height;

            Vector2 offset = new Vector2(20f, height * 0.5f + 20f);
            Vector2 targetPos = localPos + offset;

            Vector2 canvasSize = canvasRect.rect.size;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            targetPos.x = Mathf.Clamp(
                targetPos.x,
                -canvasSize.x / 2f + halfW + 10f,
                canvasSize.x / 2f - halfW - 10f
            );

            targetPos.y = Mathf.Clamp(
                targetPos.y,
                -canvasSize.y / 2f + halfH + 10f,
                canvasSize.y / 2f - halfH - 10f
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

            currentOwner = owner;

            var def = inst.itemDefinition;

            icon.sprite = def.icon;
            title.text = def.itemName;
            description.text = def.description;
            stats.text = "";

            // LEVEL
            if (inst.level > 0)
                stats.text += $"<color=#FFD700>Level {inst.level}</color>\n\n";

            // BASE BUFFS
            if (def.equippedBuffs != null)
            {
                foreach (var buff in def.equippedBuffs)
                    AppendBuff(buff);
            }

            // UPGRADE BUFFS
            if (def.upgrades != null &&
                inst.level >= 0 &&
                inst.level < def.upgrades.Length)
            {
                var upgrade = def.upgrades[inst.level];

                if (upgrade != null && upgrade.levelBuffs != null)
                {
                    stats.text += "\n";
                    foreach (var buff in upgrade.levelBuffs)
                        AppendBuff(buff);
                }
            }

            if (def.isStackable)
                stats.text += $"\nStack: {inst.quantity}/{def.maxStackAmount}";

            Show();
        }

        private void AppendBuff(BuffSO buff)
        {
            if (buff == null)
                return;

            foreach (var effect in buff.effects)
            {
                stats.text += FormatEffect(effect) + "\n";
            }
        }

        private string FormatEffect(BuffEffectSO effect)
        {
            switch (effect)
            {
                case AddStatEffectSO add:
                    return FormatAdd(add);

                case MultiplyStatEffectSO mult:
                    return FormatMultiply(mult);

                default:
                    return effect.name;
            }
        }

        private string FormatAdd(AddStatEffectSO add)
        {
            string statName = GetStatDisplayName(add.statId);

            string sign = add.value >= 0 ? "+" : "";
            string color = add.value >= 0 ? "#55FF55" : "#FF5555";

            return $"<color={color}>{sign}{add.value}</color> {statName}";
        }


        private string FormatMultiply(MultiplyStatEffectSO mult)
        {
            string statName = GetStatDisplayName(mult.StatId);

            float percent = (mult.Multiplier - 1f) * 100f;
            string sign = percent >= 0 ? "+" : "";
            string color = percent >= 0 ? "#55FF55" : "#FF5555";

            return $"<color={color}>{sign}{percent:0.#}%</color> {statName}";
        }

        private string GetStatDisplayName(string id)
        {
            return id switch
            {
                "combat.damage.mult" => "Damage",
                "combat.fireRate" => "Fire Rate",
                "combat.spread" => "Spread",
                "combat.aimSpread" => "Aim Spread",
                "combat.range" => "Range",
                "combat.recoil" => "Recoil",
                "combat.magazine" => "Magazine Size",

                "health.max" => "Max HP",
                "health.regen" => "HP Regen",

                "energy.max" => "Max Energy",
                "energy.regen" => "Energy Regen",

                "move.walk" => "Walk Speed",
                "move.sprint" => "Sprint Speed",

                "mining.power" => "Mining Power",

                _ => id
            };
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

        public void ShowBuff(BuffSO cfg)
        {
            if (cfg == null)
            {
                Hide();
                return;
            }

            currentOwner = cfg;

            icon.sprite = cfg.icon;
            title.text = cfg.displayName;
            description.text = cfg.description;
            stats.text = cfg.isDebuff
                ? "<color=#FF5555>Debuff</color>"
                : "<color=#55FF55>Buff</color>";

            foreach (var effect in cfg.effects)
                stats.text += "\n" + FormatEffect(effect);

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
