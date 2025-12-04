using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;

namespace Features.Stats.UnityIntegration
{
    public class PlayerStats : MonoBehaviour
    {
        public StatsPresetSO preset;

        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public StatsFacadeAdapter GetFacadeAdapter() => Adapter;
        public IStatsFacade Stats => Facade;

        public static event System.Action<PlayerStats> OnStatsReady;

        private void Awake()
        {
            Facade = new StatsFacade();

            // === СОЗДАЕМ ВСЕ АДАПТЕРЫ ДИНАМИЧЕСКИ ===
            Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            Adapter.Init(Facade);

            // === применяем базовые значения ===
            if (preset != null)
            {
                Facade.Combat.ApplyBase(preset.combat.baseDamageMultiplier);
                Facade.Energy.ApplyBase(preset.energy.baseMaxEnergy, preset.energy.baseRegen);
                Facade.Health.ApplyBase(preset.health.baseHp);
                Facade.Health.ApplyRegenBase(preset.health.baseRegen);
                Facade.Movement.ApplyBase(
                    preset.movement.baseSpeed,
                    preset.movement.walkSpeed,
                    preset.movement.sprintSpeed,
                    preset.movement.crouchSpeed
                );
                Facade.Mining.ApplyBase(preset.mining.baseMining);
                Debug.Log($"[PlayerStats] After ApplyBase: Facade.Energy Current={Facade.Energy.CurrentEnergy} Max={Facade.Energy.MaxEnergy}");
    
            }


            PlayerRegistry.Instance?.Register(gameObject, Adapter);
            OnStatsReady?.Invoke(this);
        }
    }
}
