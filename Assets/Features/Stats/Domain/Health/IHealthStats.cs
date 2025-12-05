using System;
using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface IHealthStats
    {
        float MaxHp { get; }
        float CurrentHp { get; }

        float MaxShield { get; }
        float CurrentShield { get; }

        /// <summary>
        /// Финальная регенерация HP (учитывает бафы + множители)
        /// </summary>
        float FinalRegen { get; }

        event Action<float, float> OnHealthChanged;
        event Action<float, float> OnShieldChanged;

        void ApplyBase(float hp);
        void ApplyShieldBase(float shield);
        void ApplyRegenBase(float regen);

        void Damage(float amount);
        void Heal(float amount);

        /// <summary>
        /// Используется системой регена.
        /// </summary>
        void Recover(float amount);

        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
