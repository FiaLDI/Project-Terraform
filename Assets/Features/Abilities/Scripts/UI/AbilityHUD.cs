using UnityEngine;
using UnityEngine.UI;
using Features.Abilities.Domain;
using Features.Abilities.UnityIntegration;

namespace Features.Abilities.UI
{
    public class AbilityHUD : MonoBehaviour
    {
        [Header("Ability Slots")]
        public AbilitySlotUI[] slots;

        [Header("Energy")]
        public PlayerEnergy energy;
        public Image energyFill;

        [Header("Channel UI")]
        public GameObject channelRoot;
        public Image channelFill;

        private AbilityCaster caster;
        private ClassManager classManager;

        private AbilitySO _currentChannelAbility;

        private void Awake()
        {
            caster = FindFirstObjectByType<AbilityCaster>();
            classManager = FindFirstObjectByType<ClassManager>();

            if (!caster)
                Debug.LogError("AbilityHUD: AbilityCaster not found");

            if (!classManager)
                Debug.LogError("AbilityHUD: ClassManager not found");
        }

        private void Start()
        {
            // === ENERGY ===
            if (energy != null)
                energy.OnEnergyChanged += UpdateEnergyView;

            // === CASTER EVENTS ===
            if (caster != null)
            {
                caster.OnChannelStarted += HandleChannelStarted;
                caster.OnChannelProgress += HandleChannelProgress;
                caster.OnChannelCompleted += HandleChannelCompleted;
                caster.OnChannelInterrupted += HandleChannelInterrupted;
            }

            if (channelRoot != null)
                channelRoot.SetActive(false);

            // === CLASS CHANGES ===
            if (classManager != null)
            {
                classManager.OnAbilitiesChanged += RebindAbilities;
                RebindAbilities();   // первый раз
            }
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
            }

            if (classManager != null)
                classManager.OnAbilitiesChanged -= RebindAbilities;
        }

        // =====================================================================
        // REBIND ALL ABILITIES (AFTER CLASS CHANGE)
        // =====================================================================
        private void RebindAbilities()
        {
            if (caster == null || classManager == null || slots == null)
                return;

            var active = classManager.ActiveAbilities;

            for (int i = 0; i < slots.Length; i++)
            {
                var ability = (i < active.Length ? active[i] : null);
                slots[i].Bind(ability, caster, i);
            }

            Debug.Log("[AbilityHUD] RebindAbilities() — updated from ClassManager");
        }

        // =====================================================================
        // ENERGY
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
            if (_currentChannelAbility != ability)
                return;

            if (channelFill != null && duration > 0)
                channelFill.fillAmount = time / duration;
        }

        private void HandleChannelCompleted(AbilitySO ability)
        {
            if (_currentChannelAbility != ability)
                return;

            if (channelRoot != null)
                channelRoot.SetActive(false);

            _currentChannelAbility = null;
        }

        private void HandleChannelInterrupted(AbilitySO ability)
        {
            if (_currentChannelAbility != ability)
                return;

            if (channelRoot != null)
                channelRoot.SetActive(false);

            _currentChannelAbility = null;
        }
    }
}
