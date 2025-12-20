using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Abilities.Domain;
using Features.Abilities.Application;
using Features.Menu.Tooltip;

namespace Features.Abilities.UI
{
    public class AbilitySlotUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private Slider cooldownSlider;
        [SerializeField] private TextMeshProUGUI keyLabel;

        [Header("Channel Highlight")]
        [SerializeField] private GameObject channelHighlight;
        [SerializeField] private Image channelProgressFill;

        [HideInInspector] public AbilitySO boundAbility;

        private AbilityCaster caster;
        private int index;

        private Sprite defaultIcon;
        private bool warnedMissingRefs = false;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            if (icon != null)
                defaultIcon = icon.sprite;

            if (channelHighlight != null)
                channelHighlight.SetActive(false);
            else
                WarnMissing(nameof(channelHighlight));

            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
            else
                WarnMissing(nameof(channelProgressFill));
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // ======================================================
        // BINDING
        // ======================================================

        public void Bind(AbilitySO ability, AbilityCaster caster, int index)
        {
            Unsubscribe();

            boundAbility = ability;
            this.caster = caster;
            this.index = index;

            keyLabel?.SetText((index + 1).ToString());

            if (ability == null)
            {
                SetEmptySlotState();
                return;
            }

            SetupIcon(ability);
            SetupCooldown(ability);
            Subscribe();
        }

        // ======================================================
        // VISUAL SETUP
        // ======================================================

        private void SetEmptySlotState()
        {
            if (icon != null)
            {
                icon.sprite = defaultIcon;
                icon.color = Color.gray;
            }

            if (cooldownSlider != null)
            {
                cooldownSlider.minValue = 0;
                cooldownSlider.maxValue = 1;
                cooldownSlider.value = 1;
            }

            if (channelHighlight != null)
                channelHighlight.SetActive(false);

            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        private void SetupIcon(AbilitySO ability)
        {
            if (icon == null) return;

            if (ability.icon != null)
            {
                icon.sprite = ability.icon;
                icon.color = Color.white;
            }
            else
            {
                icon.sprite = defaultIcon;
                icon.color = Color.yellow;
            }
        }

        private void SetupCooldown(AbilitySO ability)
        {
            if (cooldownSlider == null) return;

            cooldownSlider.minValue = 0;
            cooldownSlider.maxValue = ability.cooldown;
            cooldownSlider.value = ability.cooldown;
        }

        // ======================================================
        // EVENTS
        // ======================================================

        private void Subscribe()
        {
            if (caster == null) return;

            caster.OnAbilityCast += HandleCastReset;
            caster.OnCooldownChanged += HandleCooldownUpdate;

            caster.OnChannelStarted += HandleChannelStart;
            caster.OnChannelProgress += HandleChannelProgress;
            caster.OnChannelCompleted += HandleChannelEnd;
            caster.OnChannelInterrupted += HandleChannelEnd;
        }

        private void Unsubscribe()
        {
            if (caster == null) return;

            caster.OnAbilityCast -= HandleCastReset;
            caster.OnCooldownChanged -= HandleCooldownUpdate;

            caster.OnChannelStarted -= HandleChannelStart;
            caster.OnChannelProgress -= HandleChannelProgress;
            caster.OnChannelCompleted -= HandleChannelEnd;
            caster.OnChannelInterrupted -= HandleChannelEnd;
        }

        // ======================================================
        // COOLDOWN
        // ======================================================

        private void HandleCastReset(AbilitySO usedAbility)
        {
            if (usedAbility != boundAbility) return;
            cooldownSlider?.SetValueWithoutNotify(0);
        }

        private void HandleCooldownUpdate(AbilitySO updated, float remaining, float max)
        {
            if (updated != boundAbility) return;
            if (cooldownSlider != null)
                cooldownSlider.value = max - remaining;
        }

        // ======================================================
        // CHANNEL
        // ======================================================

        private void HandleChannelStart(AbilitySO ability)
        {
            if (ability != boundAbility) return;

            if (channelHighlight != null)
            {
                channelHighlight.SetActive(true);
            }
            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        private void HandleChannelProgress(AbilitySO ability, float time, float duration)
        {
            if (ability != boundAbility) return;

            if (duration > 0f && channelProgressFill != null)
                channelProgressFill.fillAmount = time / duration;
        }

        private void HandleChannelEnd(AbilitySO ability)
        {
            if (ability != boundAbility) return;

            if (channelHighlight != null)
            {
                channelHighlight.SetActive(false);
            }
            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        // ======================================================
        // TOOLTIP
        // ======================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundAbility == null)
                return;

            TooltipController.Instance?.ShowAbility(boundAbility);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance?.Hide();
        }

        // ======================================================
        // UTILS
        // ======================================================

        private void WarnMissing(string field)
        {
            if (warnedMissingRefs) return;
            warnedMissingRefs = true;
        }
    }
}
