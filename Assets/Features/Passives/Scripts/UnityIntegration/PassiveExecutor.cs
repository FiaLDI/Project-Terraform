using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.UnityIntegration;
using Features.Passives.Domain;

namespace Features.Passives.UnityIntegration
{
    /// <summary>
    /// Единственная точка применения пассивок.
    /// Не хранит состояние.
    /// Server-only.
    /// </summary>
    public sealed class PassiveExecutor : MonoBehaviour
    {
        public static PassiveExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Добавляет buff через BuffSystem.
        /// </summary>
        public void Apply(
            PassiveEffectData data,
            StatsBuffTarget target,
            IBuffSource source)
        {
            if (data.buff == null || target == null)
                return;

            target.BuffSystem.Add(
                data.buff,
                source,
                data.lifetime
            );
        }
    }
}
