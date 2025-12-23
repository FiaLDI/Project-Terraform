using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;
using Features.UI; // PlayerBoundUIView

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
        private EnergyStatsAdapter energy;
        private PlayerStats boundStats;
        private AbilitySO currentChannelAbility;

        // =====================================================
        // PLAYER BIND (FROM BASE)
        // =====================================================

        protected override void OnPlayerBound(GameObject player)
        {
            boundStats = player.GetComponent<PlayerStats>();
            if (boundStats == null)
                return;

            if (boundStats.Adapter == null || !boundStats.Adapter.IsReady)
            {
                PlayerStats.OnStatsReady += HandleStatsReady;
                return;
            }

            Bind(boundStats);
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            PlayerStats.OnStatsReady -= HandleStatsReady;
            Unbind();
        }

        // =====================================================
        // STATS READY
        // =====================================================

        private void HandleStatsReady(PlayerStats stats)
        {
            if (stats != boundStats)
                return;

            PlayerStats.OnStatsReady -= HandleStatsReady;
            Bind(stats);
        }

        // =====================================================
        // BIND / UNBIND
        // =====================================================

        private void Bind(PlayerStats stats)
        {
            caster = stats.GetComponent<AbilityCaster>();
            if (caster == null)
                return;

            energy = stats.Adapter.EnergyStats;

            if (energy != null)
                energy.OnEnergyChanged += UpdateEnergyView;

            caster.OnAbilitiesChanged += RebindAbilities;
            caster.OnChannelStarted += HandleChannelStarted;
            caster.OnChannelProgress += HandleChannelProgress;
            caster.OnChannelCompleted += HandleChannelCompleted;
            caster.OnChannelInterrupted += HandleChannelInterrupted;

            RebindAbilities();
        }

        private void Unbind()
        {
            if (energy != null)
                energy.OnEnergyChanged -= UpdateEnergyView;

            if (caster != null)
            {
                caster.OnAbilitiesChanged -= RebindAbilities;
                caster.OnChannelStarted -= HandleChannelStarted;
                caster.OnChannelProgress -= HandleChannelProgress;
                caster.OnChannelCompleted -= HandleChannelCompleted;
                caster.OnChannelInterrupted -= HandleChannelInterrupted;
            }

            energy = null;
            caster = null;
            boundStats = null;
            currentChannelAbility = null;

            if (channelRoot != null)
                channelRoot.SetActive(false);
        }

        // =====================================================
        // VIEW
        // =====================================================

        private void RebindAbilities()
        {
            if (caster == null || slots == null)
                return;

            var abilities = caster.Abilities;
            for (int i = 0; i < slots.Length; i++)
            {
                var ability = (i < abilities.Count) ? abilities[i] : null;
                slots[i].Bind(ability, caster, i);
            }
        }

        private void UpdateEnergyView(float current, float max)
        {
            if (energyFill != null)
                energyFill.fillAmount = max > 0 ? current / max : 0f;
        }

        // =====================================================
        // CHANNEL
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
