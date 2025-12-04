using UnityEngine;
using Features.Abilities.Domain;
using Features.Abilities.Application;

namespace Features.Abilities.UI
{
    public class AbilityUIRoot : MonoBehaviour
    {
        public AbilitySlotUI[] slots;
        public AbilityCaster caster;

        private void Start()
        {
            caster = PlayerRegistry.Instance.LocalAbilities;
            if (!caster)
            {
                Debug.LogError("AbilityUIRoot: no caster in PlayerRegistry");
                return;
            }

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

        private void OnDestroy()
        {
            if (caster != null)
                caster.OnAbilitiesChanged -= RefreshSlots;
        }
    }
}
