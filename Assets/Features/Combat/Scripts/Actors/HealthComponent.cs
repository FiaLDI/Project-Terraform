using UnityEngine;
using Features.Combat.Domain;
using Features.Combat.Application;

namespace Features.Combat.Actors
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        public float maxHp = 100f;
        public float currentHp;

        [Header("Resistances (optional)")]
        public ResistProfile resistances;

        public System.Action<float, float> OnHealthChanged;
        public System.Action OnDeath;

        private void Awake()
        {
            currentHp = maxHp;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        // ========= NEW SYSTEM =========

        public ResistProfile GetResistProfile() => resistances;

        public void ApplyDamage(float amount, DamageType type, HitInfo info)
        {
            TakeDamage(amount, type);
        }

        public void ApplyDot(DoTEffectData dot)
        {
            // можно повесить UI-эффект
            CombatServiceProvider.Service.ApplyDot(this, dot);
        }

        // ========= OLD SYSTEM (compat) =========

        public void TakeDamage(float damageAmount, DamageType damageType)
        {
            if (damageAmount <= 0f) return;

            currentHp -= damageAmount;
            OnHealthChanged?.Invoke(currentHp, maxHp);

            if (currentHp <= 0f)
                Die();
        }

        public void Heal(float healAmount)
        {
            if (healAmount <= 0f) return;

            currentHp = Mathf.Min(maxHp, currentHp + healAmount);
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        private void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
