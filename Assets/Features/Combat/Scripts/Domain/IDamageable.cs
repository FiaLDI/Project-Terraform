namespace Features.Combat.Domain
{
    public interface IDamageable
    {
        void TakeDamage(float damageAmount, DamageType damageType);
        void Heal(float healAmount);
    }
}
