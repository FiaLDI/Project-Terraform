using Features.Combat.Application;
using Features.Weapons.Domain;

public interface IProjectile
{
    void Setup(ProjectileConfig config, CombatService combat);
}
