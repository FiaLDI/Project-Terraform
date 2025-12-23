using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class EnergyStatsAdapter : MonoBehaviour, IEnergyView
    {
        private IEnergyStats _stats;
        public bool IsReady => _stats != null;

        public float MaxEnergy => _stats?.MaxEnergy ?? 0f;
        public float CurrentEnergy => _stats?.CurrentEnergy ?? 0f;
        public float Regen => _stats?.Regen ?? 0f;
        public float CostMultiplier => _stats?.CostMultiplier ?? 1f;

        public event System.Action<float, float> OnEnergyChanged;

        private void Awake()
        {
            Debug.Log("[EnergyStatsAdapter] Awake → disabling");
            enabled = false;
        }

        public void Init(IEnergyStats stats)
        {
            Debug.Log("[EnergyStatsAdapter] Init");

            if (_stats != null)
                _stats.OnEnergyChanged -= HandleChanged;

            _stats = stats;

            if (_stats != null)
            {
                Debug.Log("[EnergyStatsAdapter] Stats assigned");

                _stats.OnEnergyChanged += HandleChanged;
                HandleChanged(_stats.CurrentEnergy, _stats.MaxEnergy);

                enabled = true;
            }
            else
            {
                Debug.LogError("[EnergyStatsAdapter] Stats is NULL");
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
