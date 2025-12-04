using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    /// <summary>
    /// Адаптер, превращающий IEnergyStats → IEnergyView для UI/HUD.
    /// AbilityCaster и AbilityService НЕ используют этот класс.
    /// </summary>
    public class EnergyStatsAdapter : MonoBehaviour, IEnergyView
    {
        private IEnergyStats _stats;

        public float MaxEnergy => _stats?.MaxEnergy ?? 0f;
        public float CurrentEnergy => _stats?.CurrentEnergy ?? 0f;
        public float Regen => _stats?.Regen ?? 0f;

        public float CostMultiplier =>
            _stats != null ? _stats.CostMultiplier : 1f;

        public event System.Action<float, float> OnEnergyChanged;

        public void Init(IEnergyStats stats)
        {
            if (_stats != null)
                _stats.OnEnergyChanged -= HandleChanged;

            _stats = stats;

            if (_stats != null)
            {
                _stats.OnEnergyChanged += HandleChanged;
                HandleChanged(_stats.CurrentEnergy, _stats.MaxEnergy);
            }
        }

        private void HandleChanged(float cur, float max)
        {
            OnEnergyChanged?.Invoke(cur, max);
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnEnergyChanged -= HandleChanged;
        }
    }
}
