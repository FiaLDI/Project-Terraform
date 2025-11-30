using UnityEngine;
using System;

using Features.Combat.Domain;

/// <summary>
/// Простейший враг, совместимый с IDamageable и турелью.
/// </summary>
namespace Features.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public event Action<EnemyHealth> OnEnemyKilled;
        public static event Action<EnemyHealth> GlobalEnemyKilled;


        private bool isDead;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            Notify();
        }

        public void TakeDamage(float damageAmount, DamageType damageType)
        {
            if (isDead) return;

            CurrentHealth -= damageAmount;
            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0;
                isDead = true;
                Notify();
                Die();
            }
            else
            {
                Notify();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;

            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
            Notify();
        }

        private void Die()
        {
            OnEnemyKilled?.Invoke(this);
            GlobalEnemyKilled?.Invoke(this);
            Destroy(gameObject);
        }

        private void Notify()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

    }
}