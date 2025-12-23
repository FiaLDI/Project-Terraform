using UnityEngine;
using Features.Combat.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;

namespace Features.Combat.UnityIntegration
{
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        private IHealthStats health;
        private bool isReady;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void OnEnable()
        {
            PlayerStats.OnStatsReady += HandleStatsReady;
        }

        private void OnDisable()
        {
            PlayerStats.OnStatsReady -= HandleStatsReady;
            health = null;
            isReady = false;
        }

        // =====================================================
        // INIT
        // =====================================================

        private void HandleStatsReady(PlayerStats stats)
        {
            // ВАЖНО: только свои статы
            if (stats.gameObject != gameObject)
                return;

            health = stats.Facade?.Health;

            if (health == null)
            {
                Debug.LogError(
                    "[PlayerDamageReceiver] IHealthStats not found",
                    this
                );
                return;
            }

            isReady = true;
        }

        // =====================================================
        // IDamageable
        // =====================================================

        public void TakeDamage(float damageAmount, DamageType damageType)
        {
            if (!isReady || health == null)
                return;

            health.Damage(damageAmount);

#if UNITY_EDITOR
            Debug.Log(
                $"[DAMAGE] Player took {damageAmount} dmg ({damageType})",
                this
            );
#endif
        }

        public void Heal(float amount)
        {
            if (!isReady || health == null)
                return;

            health.Heal(amount);
        }
    }
}
