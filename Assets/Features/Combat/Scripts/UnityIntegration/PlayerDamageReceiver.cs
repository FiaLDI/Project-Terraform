using UnityEngine;
using Features.Combat.Domain;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;

namespace Features.Combat.UnityIntegration
{
    [RequireComponent(typeof(ServerGamePhase))]
    [RequireComponent(typeof(PlayerStats))]
    public sealed class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        private IHealthStats health;
        private ServerGamePhase phase;
        private bool isReady;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            phase = GetComponent<ServerGamePhase>();
            if (phase == null)
            {
                Debug.LogError(
                    "[PlayerDamageReceiver] ServerGamePhase missing",
                    this
                );
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            phase.OnPhaseReached += OnPhaseReached;
        }

        private void OnDisable()
        {
            if (phase != null)
                phase.OnPhaseReached -= OnPhaseReached;

            health = null;
            isReady = false;
        }

        // =====================================================
        // PHASE
        // =====================================================

        private void OnPhaseReached(GamePhase p)
        {
            if (p != GamePhase.StatsReady || isReady)
                return;

            Init();
        }

        private void Init()
        {
            var stats = GetComponent<PlayerStats>();
            health = stats?.Facade?.Health;

            if (health == null)
            {
                Debug.LogError(
                    "[PlayerDamageReceiver] IHealthStats not found",
                    this
                );
                return;
            }

            isReady = true;

#if UNITY_EDITOR
            Debug.Log("[PlayerDamageReceiver] READY (StatsReady)", this);
#endif
        }

        // =====================================================
        // IDamageable
        // =====================================================

        public void TakeDamage(float damageAmount, DamageType damageType)
        {
            // 🔒 жёсткая фазовая защита
            if (!PhaseAssert.Require(phase, GamePhase.StatsReady, this))
                return;

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
            if (!PhaseAssert.Require(phase, GamePhase.StatsReady, this))
                return;

            if (!isReady || health == null)
                return;

            health.Heal(amount);
        }
    }
}
