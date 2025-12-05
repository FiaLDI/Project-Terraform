using System;

namespace Features.Stats.Domain
{
    /// <summary>
    /// Read-only UI интерфейс энергии.
    /// Используется HUD, AbilityHUD, EnergyBar.
    /// </summary>
    public interface IEnergyView
    {
        float MaxEnergy { get; }
        float CurrentEnergy { get; }
        float Regen { get; }

        float CostMultiplier { get; }

        event Action<float, float> OnEnergyChanged;
    }
}
