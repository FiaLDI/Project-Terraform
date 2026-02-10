namespace Features.Stats.Domain
{
    public interface ICombatStats
    {
        float DamageMultiplier { get; }
        void ApplyBase(float dmg);
        void Reset();
    }
}
