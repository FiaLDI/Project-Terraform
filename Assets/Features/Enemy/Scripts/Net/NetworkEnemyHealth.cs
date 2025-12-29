using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Features.Combat.Domain;

namespace Features.Enemy.UnityIntegration
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class NetworkEnemyHealth : NetworkBehaviour, IDamageable
    {
        private EnemyHealth _logic;

        // SyncVar инициализируется здесь и никогда не null
        public readonly SyncVar<float> CurrentHealth = new SyncVar<float>();

        private void Awake()
        {
            _logic = GetComponent<EnemyHealth>();
            _logic.IsNetworkControlled = true;
            _logic.OnDeathRequest += HandleDeathRequest;

            // Подписываемся на события изменения
            CurrentHealth.OnChange += OnCurrentHealthChanged;
        }

        private void OnDestroy()
        {
            // УБРАНА проверка на null, так как CurrentHealth гарантированно существует
            CurrentHealth.OnChange -= OnCurrentHealthChanged;

            if (_logic != null)
            {
                _logic.OnDeathRequest -= HandleDeathRequest;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Устанавливаем начальное значение
            CurrentHealth.Value = _logic.MaxHealth;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Синхронизируем визуальное состояние при входе
            _logic.SetHealthFromNetwork(CurrentHealth.Value);
        }

        // --- IDamageable ---

        public void TakeDamage(float amount, DamageType type)
        {
            if (!base.IsServerInitialized)
            {
                ServerTakeDamage(amount, type);
                return;
            }

            ApplyDamageOnServer(amount, type);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerTakeDamage(float amount, DamageType type)
        {
            ApplyDamageOnServer(amount, type);
        }

        private void ApplyDamageOnServer(float amount, DamageType type)
        {
            _logic.TakeDamage(amount, type);
            CurrentHealth.Value = _logic.CurrentHealth;
        }

        public void Heal(float amount) { /* ... */ }

        // --- Callbacks ---

        private void OnCurrentHealthChanged(float prev, float next, bool asServer)
        {
            if (asServer) return;
            _logic.SetHealthFromNetwork(next);
        }

        private void HandleDeathRequest()
        {
            if (base.IsServerInitialized)
            {
                base.Despawn(gameObject);
            }
        }
    }
}
