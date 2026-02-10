using System;

namespace Features.Stats.Domain
{
    public class HealthStats : IHealthStats, IStatModifierTarget
    {
        public static readonly StatKey MaxHpKey = StatKeys.MaxHp;
        public static readonly StatKey ShieldKey = StatKeys.Shield;
        public static readonly StatKey RegenKey = StatKeys.HpRegen;

        private float _baseHp;
        private float _baseShield;
        private float _baseRegen;

        private float _hpAdd;
        private float _hpMult = 1f;

        private float _shieldAdd;
        private float _shieldMult = 1f;

        private float _regenAdd;
        private float _regenMult = 1f;

        public float CurrentHp { get; private set; }
        public float CurrentShield { get; private set; }

        public float MaxHp => Math.Max(0f, (_baseHp + _hpAdd) * _hpMult);
        public float MaxShield => Math.Max(0f, (_baseShield + _shieldAdd) * _shieldMult);
        public float FinalRegen => Math.Max(0f, (_baseRegen + _regenAdd) * _regenMult);

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnShieldChanged;

        public void ApplyBase(float hp)
        {
            _baseHp = hp;
            CurrentHp = Math.Min(CurrentHp > 0 ? CurrentHp : MaxHp, MaxHp);
            NotifyHp();
        }

        public void ApplyShieldBase(float shield)
        {
            _baseShield = shield;
            CurrentShield = Math.Min(CurrentShield > 0 ? CurrentShield : MaxShield, MaxShield);
            NotifyShield();
        }

        public void ApplyRegenBase(float regen) => _baseRegen = regen;

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id == MaxHpKey.Id) _hpAdd += value;
            else if (key.Id == ShieldKey.Id) _shieldAdd += value;
            else if (key.Id == RegenKey.Id) _regenAdd += value;
            else return false;

            Clamp();
            return true;
        }

        public bool TryMultiply(StatKey key, float mult)
        {
            if (key.Id == MaxHpKey.Id) _hpMult *= mult;
            else if (key.Id == ShieldKey.Id) _shieldMult *= mult;
            else if (key.Id == RegenKey.Id) _regenMult *= mult;
            else return false;

            Clamp();
            return true;
        }

        public void Damage(float amount)
        {
            if (amount <= 0) return;

            if (CurrentShield > 0)
            {
                float absorbed = Math.Min(CurrentShield, amount);
                CurrentShield -= absorbed;
                amount -= absorbed;
                NotifyShield();
            }

            if (amount > 0)
            {
                CurrentHp = Math.Max(0f, CurrentHp - amount);
                NotifyHp();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0) return;
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
            NotifyHp();
        }

        public void Recover(float amount)
        {
            if (amount <= 0) return;
            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
            NotifyHp();
        }

        public void SetCurrentHp(float value)
        {
            CurrentHp = Math.Clamp(value, 0, MaxHp);
            NotifyHp();
        }

        public void SetMaxHpDirect(float hp)
        {
            _baseHp = hp;
            CurrentHp = Math.Min(CurrentHp, MaxHp);
            NotifyHp();
        }

        private void Clamp()
        {
            CurrentHp = Math.Min(CurrentHp, MaxHp);
            CurrentShield = Math.Min(CurrentShield, MaxShield);
            NotifyHp();
            NotifyShield();
        }

        private void NotifyHp() =>
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        private void NotifyShield() =>
            OnShieldChanged?.Invoke(CurrentShield, MaxShield);

        public void Reset()
        {
            _baseHp = 0f;
            _baseShield = 0f;
            _baseRegen = 0f;

            _hpAdd = 0f;
            _hpMult = 1f;
            _shieldAdd = 0f;
            _shieldMult = 1f;
            _regenAdd = 0f;
            _regenMult = 1f;

            CurrentHp = 0f;
            CurrentShield = 0f;

            NotifyHp();
            NotifyShield();
        }
    }
}
