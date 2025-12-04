using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class HealthStatsAdapter : MonoBehaviour
    {
        private IHealthStats _stats;

        public float MaxHp => _stats?.MaxHp ?? 0f;
        public float CurrentHp => _stats?.CurrentHp ?? 0f;

        public float MaxShield => _stats?.MaxShield ?? 0f;
        public float CurrentShield => _stats?.CurrentShield ?? 0f;

        public float Regen => _stats?.Regen ?? 0f;

        public event System.Action<float, float> OnHealthChanged;
        public event System.Action<float, float> OnShieldChanged;

        public void Init(IHealthStats stats)
        {
            _stats = stats;

            // �������� �� HP
            _stats.OnHealthChanged += (cur, max) =>
            {
                OnHealthChanged?.Invoke(cur, max);
            };

            // �������� �� Shield
            _stats.OnShieldChanged += (cur, max) =>
            {
                OnShieldChanged?.Invoke(cur, max);
            };
        }

        public void Damage(float amount)
        {
            _stats?.Damage(amount);
        }

        public void Heal(float amount)
        {
            _stats?.Heal(amount);
        }
    }
}
