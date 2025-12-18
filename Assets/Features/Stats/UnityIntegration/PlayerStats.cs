using UnityEngine;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Application;
using Features.Player.UnityIntegration;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    public class PlayerStats : MonoBehaviour
    {
        public IStatsFacade Facade { get; private set; }
        public StatsFacadeAdapter Adapter { get; private set; }

        public IStatsFacade Stats => Facade;
        public StatsFacadeAdapter GetFacadeAdapter() => Adapter;

        public static event System.Action<PlayerStats> OnStatsReady;

        private void Awake()
        {
            // Создаём фасад с пустыми Domain-статами
            Facade = new StatsFacade(isTurret: false);

            // Создаём адаптер для Unity-систем (UI, Movement)
            Adapter = gameObject.AddComponent<StatsFacadeAdapter>();
            Adapter.Init(Facade);

            // Регистрируем в PlayerRegistry (если нужно)
            PlayerRegistry.Instance?.Register(gameObject, Adapter);
        }

        private void Start()
        {
            // Сообщаем контроллеру классов, что фасад готов
            OnStatsReady?.Invoke(this);
        }

        // -------- Runtime Helper Values --------

        public float FinalDamage => Facade.Combat.DamageMultiplier;

        public float FinalHp => Facade.Health.MaxHp;
        public float CurrentHp => Facade.Health.CurrentHp;

        public float CurrentEnergy => Facade.Energy.CurrentEnergy;
        public float MaxEnergy => Facade.Energy.MaxEnergy;
        public float FinalEnergyRegen => Facade.Energy.Regen;

        public float BaseSpeed => Facade.Movement.BaseSpeed;
        public float WalkSpeed => Facade.Movement.WalkSpeed;
        public float SprintSpeed => Facade.Movement.SprintSpeed;
        public float CrouchSpeed => Facade.Movement.CrouchSpeed;

        public float MiningSpeed => Facade.Mining.MiningPower;

        // DEBUG
        public float Debug_HP => Facade?.Health?.CurrentHp ?? 0f;
        public float Debug_MaxHP => Facade?.Health?.MaxHp ?? 0f;

        public float Debug_Energy => Facade?.Energy?.CurrentEnergy ?? 0f;
        public float Debug_MaxEnergy => Facade?.Energy?.MaxEnergy ?? 0f;

        public float Debug_DamageMultiplier => Facade.Combat.DamageMultiplier;

        public float Debug_Speed => Facade.Movement.BaseSpeed;
    }
}
