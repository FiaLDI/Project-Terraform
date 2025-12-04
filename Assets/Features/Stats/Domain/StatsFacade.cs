using Features.Stats.Domain;

public class StatsFacade : IStatsFacade, IStatsCollection
{
    public ICombatStats Combat { get; }
    public IEnergyStats Energy { get; }
    public IHealthStats Health { get; }
    public IMovementStats Movement { get; }
    public IMiningStats Mining { get; }

    public StatsFacade()
    {
        Combat = new CombatStats();
        Energy = new EnergyStats();
        Health = new HealthStats();
        Movement = new MovementStats();
        Mining = new MiningStats();
    }

    public void Tick(float dt)
    {
        // сюда помещаются тики бафов/пассивов
        // пока пусто — и это нормально
    }
}
