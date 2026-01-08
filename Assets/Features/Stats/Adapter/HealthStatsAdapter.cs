using UnityEngine;
using System;

namespace Features.Stats.Adapter
{
    /// <summary>
    /// ViewModel здоровья.
    /// ЧИСТО визуальный слой.
    /// </summary>
    public sealed class HealthStatsAdapter : MonoBehaviour
    {
        public float CurrentHp { get; private set; }
        public float MaxHp { get; private set; }

        public float CurrentShield { get; private set; }
        public float MaxShield { get; private set; }

        public bool IsReady { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnShieldChanged;

        /// <summary>
        /// Вызывается ТОЛЬКО из StatsNetSync.
        /// </summary>
        public void SetHp(float current, float max)
        {
            current = Mathf.Clamp(current, 0f, max);

            bool changed =
                !Mathf.Approximately(CurrentHp, current) ||
                !Mathf.Approximately(MaxHp, max);

            CurrentHp = current;
            MaxHp = max;

            if (!IsReady)
                IsReady = true;

            if (changed)
                OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        /// <summary>
        /// Если используешь щит — отдельно.
        /// </summary>
        public void SetShield(float current, float max)
        {
            current = Mathf.Clamp(current, 0f, max);

            bool changed =
                !Mathf.Approximately(CurrentShield, current) ||
                !Mathf.Approximately(MaxShield, max);

            CurrentShield = current;
            MaxShield = max;

            if (!IsReady)
                IsReady = true;

            if (changed)
                OnShieldChanged?.Invoke(CurrentShield, MaxShield);
        }
    }
}
