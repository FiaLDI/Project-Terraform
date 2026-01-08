using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Stats.Adapter;
using Features.UI;
using Features.Abilities.Client;

namespace Features.Abilities.UI
{
    public sealed class AbilityHUD : PlayerBoundUIView
    {
        [Header("Slots")]
        [SerializeField] private AbilitySlotUI[] slots;

        [Header("Energy")]
        [SerializeField] private Image energyFill;

        [Header("Channel UI")]
        [SerializeField] private GameObject channelRoot;
        [SerializeField] private Image channelFill;

        private AbilityCaster caster;
        private ClientAbilityView abilityView;
        private EnergyStatsAdapter energy;
        private AbilitySO currentChannelAbility;

        // =====================================================
        // PLAYER BIND
        // =====================================================

       protected override void OnPlayerBound(GameObject player)
        {
            // =========================
            // Ability view (DATA)
            // =========================
            abilityView = player.GetComponent<ClientAbilityView>();
            if (abilityView == null)
            {
                Debug.LogError("[AbilityHUD] ClientAbilityView missing", this);
                return;
            }

            abilityView.AbilitiesChanged += RebindAbilities;
            abilityView.Bind();

            // =========================
            // Ability caster (RUNTIME)
            // =========================
            caster = player.GetComponent<AbilityCaster>();
            if (caster == null)
            {
                Debug.LogError("[AbilityHUD] AbilityCaster not found", this);
                return;
            }

            caster.OnChannelStarted += HandleChannelStarted;
            caster.OnChannelProgress += HandleChannelProgress;
            caster.OnChannelCompleted += HandleChannelCompleted;
            caster.OnChannelInterrupted += HandleChannelInterrupted;

            // =========================
            // Stats / Energy (VIEW)
            // =========================
            var statsAdapter = player.GetComponent<StatsFacadeAdapter>();
            if (statsAdapter == null)
            {
                Debug.LogError("[AbilityHUD] StatsFacadeAdapter not found", this);
                return;
            }

            energy = statsAdapter.EnergyStats;
            if (energy != null)
            {
                energy.OnEnergyChanged += UpdateEnergyView;

                // late-join safe
                if (energy.IsReady)
                    UpdateEnergyView(energy.Current, energy.Max);
            }
        }


        protected override void OnPlayerUnbound(GameObject player)
        {
            if (abilityView != null)
                abilityView.AbilitiesChanged -= RebindAbilities;
            Unbind();
        }

        // =====================================================
        // UNBIND
        // =====================================================

        private void Unbind()
        {
            if (energy != null)
                energy.OnEnergyChanged -= UpdateEnergyView;

            if (abilityView != null)
                abilityView.AbilitiesChanged -= RebindAbilities;

            if (caster != null)
            {
                caster.OnChannelStarted -= HandleChannelStarted;
                caster.OnChannelProgress -= HandleChannelProgress;
                caster.OnChannelCompleted -= HandleChannelCompleted;
                caster.OnChannelInterrupted -= HandleChannelInterrupted;
            }

            abilityView = null;
            caster = null;
            energy = null;
            currentChannelAbility = null;

            channelRoot?.SetActive(false);
        }

        // =====================================================
        // ABILITIES VIEW
        // =====================================================

        private void RebindAbilities()
        {
            if (abilityView == null)
                return;

            var abilities = abilityView.Active;

            for (int i = 0; i < slots.Length; i++)
            {
                var ability = (i < abilities.Count) ? abilities[i] : null;
                slots[i].Bind(ability, BoundPlayer.GetComponent<AbilityCaster>(), i);
            }
        }

        // =====================================================
        // ENERGY VIEW
        // =====================================================

        private void UpdateEnergyView(float current, float max)
        {
            if (energyFill != null)
                energyFill.fillAmount = max > 0f ? current / max : 0f;
        }

        // =====================================================
        // CHANNEL VIEW
        // =====================================================

        private void HandleChannelStarted(AbilitySO ability)
        {
            currentChannelAbility = ability;
            channelRoot?.SetActive(true);

            if (channelFill != null)
                channelFill.fillAmount = 0f;
        }

        private void HandleChannelProgress(AbilitySO ability, float time, float duration)
        {
            if (ability != currentChannelAbility || duration <= 0f)
                return;

            if (channelFill != null)
                channelFill.fillAmount = time / duration;
        }

        private void HandleChannelCompleted(AbilitySO ability)
        {
            if (ability != currentChannelAbility)
                return;

            channelRoot?.SetActive(false);
            currentChannelAbility = null;
        }

        private void HandleChannelInterrupted(AbilitySO ability)
        {
            if (ability != currentChannelAbility)
                return;

            channelRoot?.SetActive(false);
            currentChannelAbility = null;
        }
    }
}
