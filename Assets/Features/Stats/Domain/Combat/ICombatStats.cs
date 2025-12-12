using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface ICombatStats
    {
        float DamageMultiplier { get; }

        void ApplyBase(float dmg);
        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
