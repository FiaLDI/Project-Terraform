using UnityEngine;
using Features.Combat.Domain;
using Features.Stats.Adapter;

namespace Features.Combat.UnityIntegration
{
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        private HealthStatsAdapter _health;

        private void Awake()
        {
            _health = GetComponent<HealthStatsAdapter>();
            if (_health == null)
                Debug.LogError("[PlayerDamageReceiver] No HealthStatsAdapter found on Player!");
        }

        public void TakeDamage(float damageAmount, DamageType damageType)
        {
            if (_health == null) return;

            _health.Damage(damageAmount);
            Debug.Log($"[DAMAGE] Player took {damageAmount} dmg ({damageType})");
        }

        public void Heal(float amount)
        {
            _health?.Heal(amount);
        }
    }
}
