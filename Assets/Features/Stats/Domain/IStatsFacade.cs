namespace Features.Stats.Domain
{
    public interface IStatsFacade
    {
        ICombatStats Combat { get; }
        IEnergyStats Energy { get; }
        IHealthStats Health { get; }
        IMovementStats Movement { get; }
        IMiningStats Mining { get; }
    }
}
