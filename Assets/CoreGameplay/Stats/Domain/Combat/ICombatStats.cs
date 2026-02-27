namespace Features.Stats.Domain
{
    public interface ICombatStats
    {
        float BaseDamage { get; }
        float DamageMultiplier { get; }
        float FinalDamage { get; }

        float FireRate { get; }
        float Spread { get; }
        float AimSpread { get; }
        float Range { get; }
        float Recoil { get; }
        int MagazineSize { get; }

        void ApplyBase(
            float baseDamage,
            float fireRate,
            float spread,
            float aimSpread,
            float range,
            float recoil,
            int magazineSize
        );

        void Reset();
    }
}