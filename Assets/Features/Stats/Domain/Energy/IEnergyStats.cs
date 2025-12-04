using System;
using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface IEnergyStats
    {
        float MaxEnergy { get; }
        float Regen { get; }
        float CurrentEnergy { get; }

        /// <summary>
        /// Мультипликативный коэффициент цены: 
        /// 1 = без изменений, 0.8 = -20% стоимости и т.п.
        /// </summary>
        float CostMultiplier { get; }

        bool TrySpend(float cost);
        bool HasEnergy(float amount);

        /// <summary>Добавить «сырую» энергию (используется для регена).</summary>
        void Recover(float amount);

        void ApplyBase(float max, float regen);

        /// <summary>
        /// Универсальный вход для баффов: 
        /// MaxEnergy / Regen / CostReduction.
        /// </summary>
        void ApplyBuff(BuffSO cfg, bool apply);

        event Action<float, float> OnEnergyChanged;
    }
}
