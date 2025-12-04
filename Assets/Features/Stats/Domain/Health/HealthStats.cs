using System;
using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class HealthStats : IHealthStats
    {
        // ============================
        // BASE VALUES
        // ============================
        private float _baseHp;
        private float _baseShield;

        // ============================
        // BUFF MODIFIERS
        // ============================
        private float _hpAdd = 0f;
        private float _hpMult = 1f;

        private float _shieldAdd = 0f;
        private float _shieldMult = 1f;

        // ============================
        // RUNTIME VALUES
        // ============================
        public float CurrentHp { get; private set; }
        public float CurrentShield { get; private set; }

        // ============================
        // EVENTS
        // ============================
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnShieldChanged;

        // ============================
        // FINAL VALUES
        // ============================
        public float MaxHp =>
            Math.Max(0f, (_baseHp + _hpAdd) * _hpMult);

        public float MaxShield =>
            Math.Max(0f, (_baseShield + _shieldAdd) * _shieldMult);

        // ============================
        // BASE APPLIERS
        // ============================
        public void ApplyBase(float hp)
        {
            _baseHp = hp;

            // refresh HP but don't overflow
            CurrentHp = Math.Min(CurrentHp <= 0 ? MaxHp : CurrentHp, MaxHp);
            NotifyHp();
        }

        public void ApplyShieldBase(float shield)
        {
            _baseShield = shield;

            CurrentShield = Math.Min(CurrentShield <= 0 ? MaxShield : CurrentShield, MaxShield);
            NotifyShield();
        }

        // ============================
        // DAMAGE & HEAL
        // ============================
        public void Damage(float amount)
        {
            if (amount <= 0) return;

            // shield absorbs first
            if (CurrentShield > 0)
            {
                float absorb = Math.Min(CurrentShield, amount);
                CurrentShield -= absorb;
                amount -= absorb;
                NotifyShield();
            }

            if (amount > 0)
            {
                CurrentHp -= amount;
                if (CurrentHp < 0) CurrentHp = 0;
                NotifyHp();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0) return;

            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
            NotifyHp();
        }

        // ============================
        // APPLY BUFF (NEW SYSTEM)
        // ============================
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            float sign = apply ? 1f : -1f;

            switch (cfg.stat)
            {
                // ---------------------
                // MAX HP
                // ---------------------
                case BuffStat.PlayerDamage:
                    // ignore, belongs to CombatStats
                    return;

                case BuffStat.PlayerMoveSpeed:
                case BuffStat.PlayerMiningSpeed:
                case BuffStat.PlayerEnergyRegen:
                case BuffStat.PlayerEnergyCostReduction:
                case BuffStat.PlayerMaxEnergy:
                    // ignore — not part of HealthStats
                    return;

                // ---------------------
                // HP BUFF
                // ---------------------
                case BuffStat.HealPerSecond:
                    // handled in BuffExecutor.Tick
                    return;

                // ---------------------
                // SHIELD BUFF
                // ---------------------
                case BuffStat.PlayerShield:
                    ApplyShieldBuffInternal(cfg, sign);
                    break;

                default:
                    // HP buff is generic
                    ApplyHpBuffInternal(cfg, sign);
                    break;
            }

            ClampValues();
            NotifyHp();
            NotifyShield();
        }

        // ============================
        // INTERNAL HP BUFF LOGIC
        // ============================
        private void ApplyHpBuffInternal(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _hpAdd += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    _hpMult = sign > 0 ? _hpMult * cfg.value : _hpMult / cfg.value;
                    break;

                case BuffModType.Set:
                    _hpMult = sign > 0 ? cfg.value : 1f;
                    break;
            }
        }

        // ============================
        // INTERNAL SHIELD BUFF LOGIC
        // ============================
        private void ApplyShieldBuffInternal(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _shieldAdd += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    _shieldMult = sign > 0 ? _shieldMult * cfg.value : _shieldMult / cfg.value;
                    break;

                case BuffModType.Set:
                    _shieldMult = sign > 0 ? cfg.value : 1f;
                    break;
            }
        }

        // ============================
        // HELPERS
        // ============================
        private void ClampValues()
        {
            CurrentHp = Math.Min(CurrentHp, MaxHp);
            CurrentShield = Math.Min(CurrentShield, MaxShield);
        }

        private void NotifyHp() =>
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        private void NotifyShield() =>
            OnShieldChanged?.Invoke(CurrentShield, MaxShield);
    }
}
