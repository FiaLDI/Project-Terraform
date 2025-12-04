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
        /// Естественная регенерация HP (hp / sec)
        /// </summary>
        float Regen { get; }

        event Action<float, float> OnHealthChanged;
        event Action<float, float> OnShieldChanged;

        void ApplyBase(float hp);
        void ApplyShieldBase(float shield);

        void Damage(float amount);
        void Heal(float amount);

        /// <summary>
        /// Используется системами регенерации, например StatsUpdateSystem
        /// </summary>
        void Recover(float amount);

        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
