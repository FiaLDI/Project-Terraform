
namespace Features.Stats.Domain
{
    public interface ICombatStats : IStatModifierTarget
    {

        float DamageMultiplier { get; }
        float FireRate { get; }
        float Range { get; }
        float Spread { get; }
        float AimSpread { get; }
        float Recoil { get; }
        int MagazineSize { get; }

        void ApplyBase(
            float damageMultiplier,
            float fireRate,
            float range,
            float spread,
            float aimSpread,
            float recoil,
            int magazineSize);

        void Reset();
    }
}
