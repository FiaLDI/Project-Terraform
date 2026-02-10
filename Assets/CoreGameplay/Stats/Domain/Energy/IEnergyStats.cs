using System;

namespace Features.Stats.Domain
{
    public interface IEnergyStats
    {
        float MaxEnergy { get; }
        float Regen { get; }
        float CurrentEnergy { get; }
        float CostMultiplier { get; }

        event Action<float, float> OnEnergyChanged;

        void ApplyBase(float max, float regen);
        bool HasEnergy(float amount);
        bool TrySpend(float amount);
        void Recover(float amount);

        void SetCurrentEnergy(float value);
        void SetMaxEnergyDirect(float max);

        void Reset();
    }
}
