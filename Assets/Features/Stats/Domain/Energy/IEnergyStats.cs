using System;
using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    /// <summary>
    /// Единственный интерфейс энергии в проекте.
    /// Никаких IEnergy из Features.Energy.Domain больше не должно быть.
    /// </summary>
    public interface IEnergyStats
    {
        float MaxEnergy { get; }
        float CurrentEnergy { get; }
        float Regen { get; }

        /// <summary>
        /// Коэффициент стоимости (1 = базовая, 0.8 = -20% cost, 1.2 = +20% cost)
        /// </summary>
        float CostMultiplier { get; }

        /// <summary>
        /// Текущее/максимальное значение изменилось.
        /// </summary>
        event Action<float, float> OnEnergyChanged;

        // База от класса/предметов
        void ApplyBase(float max, float regen);

        // Бафы/дебафы
        void ApplyBuff(BuffSO cfg, bool apply);

        // Трата/проверка/восстановление
        bool HasEnergy(float amount);
        bool TrySpend(float amount);
        void Recover(float amount);
        void SetCurrentEnergy(float value);
        void SetMaxEnergyDirect(float max);
        void Reset();
    }
}
