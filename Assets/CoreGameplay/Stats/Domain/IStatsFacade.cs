namespace Features.Stats.Domain
{
    public interface IStatsFacade
    {
        IHealthStats Health { get; }
        IEnergyStats Energy { get; }
        ICombatStats Combat { get; }
        IMovementStats Movement { get; }
        IMiningStats Mining { get; }

        bool TryAdd(StatKey key, float value);
        bool TryMultiply(StatKey key, float multiplier);

        void ResetAll();
    }
}
