using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface ITurretCombatStats : ICombatStats
    {
        float FireRate { get; }

        void ApplyFireRateBase(float baseRate);
        void ApplyFireRateBuff(BuffSO cfg, bool apply);
    }
}
