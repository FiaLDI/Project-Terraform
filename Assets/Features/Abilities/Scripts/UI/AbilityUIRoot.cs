using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;

namespace Features.Abilities.UI
{
    public class AbilityUIRoot : MonoBehaviour
    {
        public AbilitySlotUI[] slots;

        private AbilityCaster caster;

        private void OnEnable()
        {
            PlayerStats.OnStatsReady += OnReady;
        }

        private void OnDisable()
        {
            PlayerStats.OnStatsReady -= OnReady;

            if (caster != null)
                caster.OnAbilitiesChanged -= RefreshSlots;
        }

        private void OnReady(PlayerStats stats)
        {
            caster = stats.GetComponent<AbilityCaster>();

            if (caster == null)
            {
                Debug.LogError("[AbilityUIRoot] AbilityCaster not found!");
                return;
            }

            caster.OnAbilitiesChanged -= RefreshSlots;
            caster.OnAbilitiesChanged += RefreshSlots;

            RefreshSlots();
        }

        private void RefreshSlots()
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
    }
}
