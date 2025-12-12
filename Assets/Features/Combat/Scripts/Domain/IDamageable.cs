namespace Features.Combat.Domain
{
    public interface IDamageable
    {
        // === OLD SYSTEM (backwards compatibility) ===

        void TakeDamage(float damageAmount, DamageType damageType);

        void Heal(float healAmount);

        // === NEW SYSTEM (CombatService 2.0) ===

        ResistProfile GetResistProfile() => new ResistProfile();

        void ApplyDamage(float amount, DamageType type, HitInfo info)
        {
            // by default forward to legacy method
            TakeDamage(amount, type);
        }

        void ApplyDot(DoTEffectData dot) { }
    }
}
