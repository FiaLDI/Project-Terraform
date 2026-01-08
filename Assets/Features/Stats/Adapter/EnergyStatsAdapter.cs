using UnityEngine;
using System;

namespace Features.Stats.Adapter
{
    /// <summary>
    /// ViewModel энергии.
    /// НЕ знает про домен, НЕ знает про сеть.
    /// Получает значения ТОЛЬКО через Set().
    /// </summary>
    public sealed class EnergyStatsAdapter : MonoBehaviour
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        public float Regen { get; private set; }
        public float CostMultiplier { get; private set; } = 1f;

        public bool IsReady { get; private set; }

        public event Action<float, float> OnEnergyChanged;

        /// <summary>
        /// Вызывается ТОЛЬКО из StatsNetSync.
        /// </summary>
        public void Set(float current, float max)
        {
            current = Mathf.Clamp(current, 0f, max);

            bool changed =
                !Mathf.Approximately(Current, current) ||
                !Mathf.Approximately(Max, max);

            Current = current;
            Max = max;

            if (!IsReady)
                IsReady = true;

            if (changed)
                OnEnergyChanged?.Invoke(Current, Max);
        }

        /// <summary>
        /// Опционально: если ты хочешь показывать реген в UI.
        /// НЕ влияет на домен.
        /// </summary>
        public void SetMeta(float regen, float costMultiplier)
        {
            Regen = regen;
            CostMultiplier = costMultiplier;
        }
    }
}
