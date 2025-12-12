using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;

namespace Features.Abilities.UI
{
    public class AbilityHUD : MonoBehaviour
    {
        public AbilitySlotUI[] slots;

        [Header("Energy")]
        public Image energyFill;

        [Header("Channel UI")]
        public GameObject channelRoot;
        public Image channelFill;

        private AbilityCaster caster;
        private EnergyStatsAdapter energy;

        private AbilitySO _currentChannelAbility;
        
        private void Start()
        {
            PlayerStats.OnStatsReady += OnReady;
        }

        private void OnReady(PlayerStats stats)
        {
            var adapter = stats.GetFacadeAdapter();

            caster = stats.GetComponent<AbilityCaster>();
            energy = adapter.EnergyStats;

            if (energy != null)
                energy.OnEnergyChanged += UpdateEnergyView;

            caster.OnChannelStarted += HandleChannelStarted;
            caster.OnChannelProgress += HandleChannelProgress;
            caster.OnChannelCompleted += HandleChannelCompleted;
            caster.OnChannelInterrupted += HandleChannelInterrupted;
            caster.OnAbilitiesChanged += RebindAbilities;

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

        private void RebindAbilities()
        {
            if (slots == null || caster == null) return;

            var abilities = caster.abilities;

            for (int i = 0; i < slots.Length; i++)
            {
                AbilitySO ability = (i < abilities.Length) ? abilities[i] : null;
                slots[i].Bind(ability, caster, i);
            }
        }

        private void UpdateEnergyView(float current, float max)
        {
            if (energyFill != null)
                energyFill.fillAmount = max > 0 ? current / max : 0;
        }

        private void HandleChannelStarted(AbilitySO ability)
        {
            _currentChannelAbility = ability;
            channelRoot?.SetActive(true);
            if (channelFill != null) channelFill.fillAmount = 0f;
        }

        private void HandleChannelProgress(AbilitySO ability, float time, float duration)
        {
            if (ability != _currentChannelAbility) return;
            if (channelFill != null && duration > 0)
                channelFill.fillAmount = time / duration;
        }

        private void HandleChannelCompleted(AbilitySO ability)
        {
            if (ability != _currentChannelAbility) return;
            channelRoot?.SetActive(false);
            _currentChannelAbility = null;
        }

        private void HandleChannelInterrupted(AbilitySO ability)
        {
            if (ability != _currentChannelAbility) return;
            channelRoot?.SetActive(false);
            _currentChannelAbility = null;
        }
    }
}
