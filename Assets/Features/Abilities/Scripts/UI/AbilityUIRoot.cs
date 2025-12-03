using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;

namespace Features.Abilities.UI
{
    public class AbilityUIRoot : MonoBehaviour
    {
        public AbilitySlotUI[] slots;
        public AbilityCaster caster;

        private void Start()
        {
            if (caster == null)
                caster = FindAnyObjectByType<AbilityCaster>();

            if (!caster)
            {
                Debug.LogError("AbilityUIRoot: no AbilityCaster found");
                return;
            }

            caster.OnAbilitiesChanged += RefreshSlots;
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (caster == null || slots == null)
                return;

            for (int i = 0; i < slots.Length; i++)
            {
                AbilitySO ability = (i < caster.abilities.Length) ? caster.abilities[i] : null;
                slots[i].Bind(ability, caster, i);
            }
        }

        private void OnDestroy()
        {
            if (caster != null)
                caster.OnAbilitiesChanged -= RefreshSlots;
        }
    }
}
