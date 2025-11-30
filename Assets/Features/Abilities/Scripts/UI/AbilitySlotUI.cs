using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;
using Features.Menu.Tooltip;

namespace Features.Abilities.UI
{
    public class AbilitySlotUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI")]
        public Image icon;
        public Slider cooldownSlider;
        public TextMeshProUGUI keyLabel;

        [Header("Channel Highlight")]
        public GameObject channelHighlight;    
        public Image channelProgressFill;

        [HideInInspector] public AbilitySO boundAbility;

        private AbilityCaster caster;
        private int index;

        private Sprite defaultIcon;

        private void Awake()
        {
            if (icon != null)
                defaultIcon = icon.sprite;

            if (channelHighlight != null)
                channelHighlight.SetActive(false);

            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        public void Bind(AbilitySO ability, AbilityCaster caster, int index)
        {
            Unsubscribe();

            this.boundAbility = ability;
            this.caster = caster;
            this.index = index;

            if (keyLabel != null)
                keyLabel.text = (index + 1).ToString();

            if (ability == null)
            {
                SetEmptySlotState();
                return;
            }

            SetupIcon(ability);
            SetupCooldown(ability);
            Subscribe();
        }

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

            icon.sprite = ability.icon != null ? ability.icon : defaultIcon;
            icon.color = ability.icon != null ? Color.white : Color.yellow;
        }

        private void SetupCooldown(AbilitySO ability)
        {
            if (cooldownSlider == null) return;

            cooldownSlider.minValue = 0;
            cooldownSlider.maxValue = ability.cooldown;
            cooldownSlider.value = ability.cooldown;
        }

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

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // ============================================================
        // COOLDOWN UI
        // ============================================================

        private void HandleCastReset(AbilitySO usedAbility)
        {
            if (usedAbility != boundAbility) return;
            if (cooldownSlider != null)
                cooldownSlider.value = 0;
        }

        private void HandleCooldownUpdate(AbilitySO updated, float remaining, float max)
        {
            if (updated != boundAbility) return;
            if (cooldownSlider != null)
                cooldownSlider.value = max - remaining;
        }

        // ============================================================
        // CHANNEL UI
        // ============================================================

        private void HandleChannelStart(AbilitySO ability)
        {
            if (ability != boundAbility) return;

            if (channelHighlight != null)
                channelHighlight.SetActive(true);

            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        private void HandleChannelProgress(AbilitySO ability, float time, float duration)
        {
            if (ability != boundAbility) return;

            if (channelProgressFill != null && duration > 0)
                channelProgressFill.fillAmount = time / duration;
        }

        private void HandleChannelEnd(AbilitySO ability)
        {
            if (ability != boundAbility) return;

            if (channelHighlight != null)
                channelHighlight.SetActive(false);

            if (channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

        // ============================================================
        // TOOLTIP
        // ============================================================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundAbility != null)
                TooltipController.Instance.ShowAbility(boundAbility, caster);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Instance.Hide();
        }

        public void SetChannelHighlight(bool active)
        {
            if (channelHighlight != null)
                channelHighlight.SetActive(active);

            if (!active && channelProgressFill != null)
                channelProgressFill.fillAmount = 0f;
        }

    }
}
