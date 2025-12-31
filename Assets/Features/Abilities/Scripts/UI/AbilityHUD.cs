using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Application;
using Features.Abilities.Domain;
using Features.Stats.Adapter;
using Features.UI;

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
        private AbilitySO currentChannelAbility;

        // =====================================================
        // PLAYER BIND
        // =====================================================

        protected override void OnPlayerBound(GameObject player)
        {
            // -------- AbilityCaster --------
            caster = player.GetComponent<AbilityCaster>();
            if (caster == null)
            {
                Debug.LogError("[AbilityHUD] AbilityCaster not found", this);
                return;
            }

            // -------- Stats Adapter --------
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

                // если снапшот уже был — обновим сразу
                if (energy.IsReady)
                    UpdateEnergyView(energy.Current, energy.Max);
            }

            // -------- Ability events --------
            caster.OnAbilitiesChanged += RebindAbilities;
            caster.OnChannelStarted += HandleChannelStarted;
            caster.OnChannelProgress += HandleChannelProgress;
            caster.OnChannelCompleted += HandleChannelCompleted;
            caster.OnChannelInterrupted += HandleChannelInterrupted;

            // биндим способности сразу (или дождёмся OnAbilitiesChanged)
            RebindAbilities();
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            Unbind();
        }

        // =====================================================
        // UNBIND
        // =====================================================

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
            currentChannelAbility = null;

            if (channelRoot != null)
                channelRoot.SetActive(false);
        }

        // =====================================================
        // ABILITIES VIEW
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
