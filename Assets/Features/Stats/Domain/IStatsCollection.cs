namespace Features.Stats.Domain
{
    public interface IStatsCollection
    {
        IEnergyStats Energy { get; }
        IHealthStats Health { get; }

        void Tick(float dt); // обновление бафов / дебафов / пассивов
    }
}