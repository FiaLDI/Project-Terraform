using Features.Stats.Data;

namespace Features.Stats.Domain
{
    public class StatsFacade : IStatsFacade, IStatsCollection
    {
        public ICombatStats Combat { get; }
        public IEnergyStats Energy { get; }
        public IHealthStats Health { get; }
        public IMovementStats Movement { get; }
        public IMiningStats Mining { get; }

        public StatsFacade(StatsProfileSO profile)
        {
            if (profile.hasCombat)
                Combat = profile.useTurretCombat
                    ? new TurretCombatStats()
                    : new CombatStats();

            if (profile.hasEnergy)
                Energy = new EnergyStats();

            if (profile.hasHealth)
                Health = new HealthStats();

            if (profile.hasMovement)
                Movement = new MovementStats();

            if (profile.hasMining)
                Mining = new MiningStats();
        }

        public void ResetAll()
        {
            Health?.Reset();
            Energy?.Reset();
            Combat?.Reset();
            Movement?.Reset();
            Mining?.Reset();
        }

        public void Tick(float dt) { }
    }
}
