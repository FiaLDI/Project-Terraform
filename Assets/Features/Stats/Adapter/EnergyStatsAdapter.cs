using Features.Energy.Domain;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Stats.Adapter
{
    public class EnergyStatsAdapter : MonoBehaviour, IEnergy, IEnergyCostModifier
    {
        private IEnergyStats _stats;

        public float CurrentEnergy => _stats.CurrentEnergy;
        public float MaxEnergy => _stats.MaxEnergy;
        public float Regen => _stats.Regen;

        public event System.Action<float, float> OnEnergyChanged;

        public void Init(IEnergyStats stats)
        {
            _stats = stats;

            _stats.OnEnergyChanged += (cur, max) =>
            {
                OnEnergyChanged?.Invoke(cur, max);
            };

            OnEnergyChanged?.Invoke(_stats.CurrentEnergy, _stats.MaxEnergy);
        }

        // --------------------------
        // Новый метод для костов
        // --------------------------
        public float ModifyCost(float baseCost)
        {
            return baseCost * (_stats as EnergyStats).CostMultiplier;
        }

        public bool HasEnergy(float amount) => _stats.HasEnergy(amount);

        public bool TrySpend(float amount)
        {
            bool ok = _stats.TrySpend(amount);
            return ok;
        }
    }
}
