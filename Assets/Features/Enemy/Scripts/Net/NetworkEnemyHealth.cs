using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Combat.Domain;

namespace Features.Enemy.UnityIntegration
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class NetworkEnemyHealth : NetworkBehaviour, IDamageable
    {
        private EnemyHealth logic;

        public readonly SyncVar<float> CurrentHealth = new();

        private void Awake()
        {
            logic = GetComponent<EnemyHealth>();
            CurrentHealth.OnChange += OnHealthChanged;
        }

        private void OnDestroy()
        {
            CurrentHealth.OnChange -= OnHealthChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            CurrentHealth.Value = logic.MaxHealth;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            logic.SetHealthFromNetwork(CurrentHealth.Value);
        }

        // ================= DAMAGE =================

        public void TakeDamage(float amount, DamageType type)
        {
            if (IsServer)
            {
                ApplyDamage(amount, type);
            }
            else
            {
                TakeDamage_Server(amount, type);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamage_Server(float amount, DamageType type)
        {
            ApplyDamage(amount, type);
        }

        [Server]
        private void ApplyDamage(float amount, DamageType type)
        {
            logic.ApplyDamageServer(amount);
            CurrentHealth.Value = logic.CurrentHealth;

            if (logic.CurrentHealth <= 0f)
                Despawn(gameObject);
        }

        // ================= SYNC =================

        private void OnHealthChanged(float prev, float next, bool asServer)
        {
            if (asServer) return;
            logic.SetHealthFromNetwork(next);
        }

        public void Heal(float healAmount)
        {
            throw new System.NotImplementedException();
        }
    }
}
