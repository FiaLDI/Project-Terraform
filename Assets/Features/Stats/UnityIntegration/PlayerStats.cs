using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    public class PlayerStats : MonoBehaviour
    {
        public StatsPresetSO preset;

        public IStatsFacade Stats { get; private set; }

        private StatsFacadeAdapter _adapter;

        private void Awake()
        {
            // создаём доменную часть
            Stats = new StatsFacade();

            // применяем пресет
            if (preset != null)
            {
                Stats.Combat.ApplyBase(preset.combat.baseDamageMultiplier);
                Stats.Energy.ApplyBase(preset.energy.baseMaxEnergy, preset.energy.baseRegen);
                Stats.Health.ApplyBase(preset.health.baseHp);
                Stats.Movement.ApplyBase(
                    preset.movement.baseSpeed,
                    preset.movement.walkSpeed,
                    preset.movement.sprintSpeed,
                    preset.movement.crouchSpeed
                );
                Stats.Mining.ApplyBase(preset.mining.baseMining);
            }

            // создаём адаптер фасада (ТОЛЬКО 1 раз)
            _adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            _adapter.Init(Stats);
        }

        public StatsFacadeAdapter GetFacadeAdapter() => _adapter;
    }
}
