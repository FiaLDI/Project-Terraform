namespace Features.Stats.Domain
{
    public interface ITurretCombatStats
    {
        float FireRate { get; }
        void ApplyFireRateBase(float baseRate);
        void Reset();
    }
}
