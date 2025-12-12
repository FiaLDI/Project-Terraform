using UnityEngine;
using System;
using Features.Combat.Domain;
using Features.Enemy.Data;

namespace Features.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
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
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(float amount, DamageType type)
        {
            if (isDead) return;

            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                isDead = true;
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
                GlobalEnemyKilled?.Invoke(this);
                Destroy(gameObject);
            }
            else
            {
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        public void Heal(float healAmount)
        {
            throw new NotImplementedException();
        }
    }
}
