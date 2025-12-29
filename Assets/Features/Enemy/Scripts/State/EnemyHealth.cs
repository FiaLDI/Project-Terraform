using UnityEngine;
using System;
using Features.Combat.Domain;
using Features.Enemy.Data;

namespace Features.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public EnemyConfigSO config;

        public string EnemyId => config.enemyId;

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public static event Action<EnemyHealth> GlobalEnemyKilled;
        
        // Новое событие: "Я хочу умереть", но решать будет Network скрипт
        public event Action OnDeathRequest; 

        private bool isDead;

        // Флаг, чтобы отличать локальный урон от сетевого
        public bool IsNetworkControlled { get; set; } = false;

        private void Awake()
        {
            MaxHealth = config.baseMaxHealth;
            CurrentHealth = MaxHealth;
            // Не вызываем ивент тут, чтобы не спамить при инициализации
        }

        public void TakeDamage(float amount, DamageType type)
        {
            if (isDead) return;

            // Если мы под полным контролем сети (клиент), мы игнорируем прямой вызов TakeDamage,
            // если только он не пришел из NetworkEnemyHealth. 
            // Но проще разрешить, а NetworkAdapter будет решать, кто главный.
            
            ApplyDamageLogic(amount);
        }

        /// <summary>
        /// Внутренняя логика применения урона
        /// </summary>
        private void ApplyDamageLogic(float amount)
        {
            CurrentHealth -= amount;
            
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                isDead = true;
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
                GlobalEnemyKilled?.Invoke(this);
                
                // ВМЕСТО Destroy(gameObject):
                if (IsNetworkControlled)
                {
                    // Сообщаем сетевому адаптеру, что пора умирать
                    OnDeathRequest?.Invoke();
                }
                else
                {
                    // Оффлайн режим (или если адаптера нет)
                    Destroy(gameObject);
                }
            }
            else
            {
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        /// <summary>
        /// Метод для синхронизации здоровья с Сервера на Клиент.
        /// Устанавливает значения "молча" или с вызовом визуальных событий.
        /// </summary>
        public void SetHealthFromNetwork(float health)
        {
            // Если здоровье уменьшилось, можно проиграть анимацию получения урона,
            // но саму логику смерти оставим серверу.
            float prevHealth = CurrentHealth;
            CurrentHealth = health;
            
            // Если здоровье изменилось, обновляем UI/Events
            if (Math.Abs(prevHealth - health) > 0.01f)
            {
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
            
            // Если сервер сказал "0", значит мы умерли
            if (CurrentHealth <= 0 && !isDead)
            {
                isDead = true;
                GlobalEnemyKilled?.Invoke(this);
                // Тут Destroy не зовем, сервер сам Despawn сделает
            }
        }

        public void Heal(float healAmount)
        {
            throw new NotImplementedException();
        }
    }
}
