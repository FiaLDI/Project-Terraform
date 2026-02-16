using System;

namespace Features.Stats.Domain
{
    public interface IHealthStats
    {
        float MaxHp { get; }
        float MaxShield { get; }
        float CurrentHp { get; }
        float CurrentShield { get; }
        float FinalRegen { get; }

        event Action<float, float> OnHealthChanged;
        event Action<float, float> OnShieldChanged;

        void ApplyBase(float hp);
        void ApplyShieldBase(float shield);
        void ApplyRegenBase(float regen);

        void Damage(float amount);
        void Heal(float amount);
        void Recover(float amount);

        void SetCurrentHp(float value);
        void SetMaxHpDirect(float hp);

        void Reset();
    }
}
