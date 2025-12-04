using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Domain;
using Features.Energy.Domain;
using Features.Abilities.UnityIntegration;
using Features.Abilities.Application;

namespace Features.Abilities.UI
{
    public class AbilityHUD : MonoBehaviour
    {
        [Header("Ability Slots")]
        public AbilitySlotUI[] slots;

        [Header("Energy")]
        public MonoBehaviour energySource; // MUST IMPLEMENT IEnergy
        public Image energyFill;

        [Header("Channel UI")]
        public GameObject channelRoot;
        public Image channelFill;

        private IEnergy energy;
        private AbilityCaster caster;
        private AbilitySO _currentChannelAbility;

        private void Awake()
        {
            caster = FindAnyObjectByType<AbilityCaster>();
            energy = energySource as IEnergy;

            if (!caster)
                Debug.LogError("AbilityHUD: AbilityCaster not found");

            if (energySource != null && energy == null)
                Debug.LogError("AbilityHUD: energySource does not implement IEnergy");
        }

        private void Start()
        {
            // ENERGY UI
            if (energy != null)
                energy.OnEnergyChanged += UpdateEnergyView;

            // CASTER EVENTS
            if (caster != null)
            {
                caster.OnChannelStarted += HandleChannelStarted;
                caster.OnChannelProgress += HandleChannelProgress;
                caster.OnChannelCompleted += HandleChannelCompleted;
                caster.OnChannelInterrupted += HandleChannelInterrupted;

                caster.OnAbilitiesChanged += RebindAbilities;
            }

            if (channelRoot != null)
                channelRoot.SetActive(false);

            RebindAbilities();
        }

        private void OnDestroy()
        {
            if (energy != null)
                energy.OnEnergyChanged -= UpdateEnergyView;

            if (caster != null)
            {
                caster.OnChannelStarted -= HandleChannelStarted;
                caster.OnChannelProgress -= HandleChannelProgress;
                caster.OnChannelCompleted -= HandleChannelCompleted;
                caster.OnChannelInterrupted -= HandleChannelInterrupted;

                caster.OnAbilitiesChanged -= RebindAbilities;
            }
        }

        // =====================================================================
        // ABILITIES UI
        // =====================================================================
        private void RebindAbilities()
        {
            if (caster == null || slots == null)
                return;

            var abilities = caster.abilities;

            for (int i = 0; i < slots.Length; i++)
            {
                AbilitySO ability = (i < abilities.Length) ? abilities[i] : null;
                slots[i].Bind(ability, caster, i);
            }
        }

        // =====================================================================
        // ENERGY UI
        // =====================================================================
        private void UpdateEnergyView(float current, float max)
        {
            if (energyFill != null && max > 0)
                energyFill.fillAmount = current / max;
        }

        // =====================================================================
        // CHANNEL UI
        // =====================================================================
        private void HandleChannelStarted(AbilitySO ability)
        {
            _currentChannelAbility = ability;

            if (channelRoot != null)
                channelRoot.SetActive(true);

            if (channelFill != null)
                channelFill.fillAmount = 0f;
        }

        private void HandleChannelProgress(AbilitySO ability, float time, float duration)
        {
            if (ability != _currentChannelAbility) return;

            if (channelFill != null && duration > 0f)
                channelFill.fillAmount = time / duration;
        }

        private void HandleChannelCompleted(AbilitySO ability)
        {
            if (ability != _currentChannelAbility) return;

            if (channelRoot != null)
                channelRoot.SetActive(false);

            _currentChannelAbility = null;
        }

        private void HandleChannelInterrupted(AbilitySO ability)
        {
            if (ability != _currentChannelAbility) return;

            if (channelRoot != null)
                channelRoot.SetActive(false);

            _currentChannelAbility = null;
        }
    }
}
