using UnityEngine;
using System;
using Features.Combat.Domain;
using Features.Enemy.Data;

namespace Features.Enemy
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [Header("Config")]
        public EnemyConfigSO config;

        public string EnemyId => config.enemyId;

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public static event Action<EnemyHealth> GlobalEnemyKilled;

        private bool isDead;

        private void Awake()
        {
            MaxHealth = config.baseMaxHealth;
            CurrentHealth = MaxHealth;
            isDead = false;
        }

        // =====================================================
        // SERVER-ONLY LOGIC
        // =====================================================

        /// <summary>
        /// Единственный допустимый способ изменить HP.
        /// Вызывается ТОЛЬКО сервером (через NetworkEnemyHealth).
        /// </summary>
        public void ApplyDamageServer(float amount)
        {
            if (isDead)
                return;

            CurrentHealth -= amount;

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                isDead = true;

                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
                GlobalEnemyKilled?.Invoke(this);
            }
            else
            {
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        // =====================================================
        // CLIENT VISUAL SYNC
        // =====================================================

        /// <summary>
        /// Вызывается ТОЛЬКО с клиента при обновлении SyncVar.
        /// Никакой логики смерти, только визуал.
        /// </summary>
        public void SetHealthFromNetwork(float health)
        {
            float prev = CurrentHealth;
            CurrentHealth = Mathf.Clamp(health, 0f, MaxHealth);

            if (Mathf.Abs(prev - CurrentHealth) > 0.01f)
            {
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }

            if (CurrentHealth <= 0f)
            {
                isDead = true;
            }
        }
    }
}
