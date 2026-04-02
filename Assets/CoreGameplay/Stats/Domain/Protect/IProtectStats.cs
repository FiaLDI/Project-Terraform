using Features.Stats.Domain;

namespace Features.Stats.Domain
{
    public interface IProtectStats
    {
        float GenericResistance { get; }
        float ExplosionResistance { get; }
        float EnergyResistance { get; }
        float MiningResistance { get; }
        float MeleeResistance { get; }
        float FireResistance { get; }
        float ElectricResistance { get; }
        float PoisonResistance { get; }
        float FrostResistance { get; }
        float AcidResistance { get; }

        void ApplyBase(
            float genericResistance,
            float explosionResistance,
            float energyResistance,
            float miningResistance,
            float meleeResistance,
            float fireResistance,
            float electricResistance,
            float poisonResistance,
            float frostResistance,
            float acidResistance
        );

        bool TryAdd(StatKey key, float value);
        bool TryMultiply(StatKey key, float value);

        void Reset();
    }
}
