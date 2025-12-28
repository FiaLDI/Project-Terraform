using System;
using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Stats.Domain
{
    public class HealthStats : IHealthStats
    {
        private float _baseHp;
        private float _baseShield;

        private float _regen = 0f; // base regen

        private float _hpAdd = 0f;
        private float _hpMult = 1f;

        private float _shieldAdd = 0f;
        private float _shieldMult = 1f;

        private float _regenAdd = 0f;
        private float _regenMult = 1f;

        public float CurrentHp { get; private set; }
        public float CurrentShield { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnShieldChanged;

        public float MaxHp =>
            Math.Max(0f, (_baseHp + _hpAdd) * _hpMult);

        public float MaxShield =>
            Math.Max(0f, (_baseShield + _shieldAdd) * _shieldMult);

        public float FinalRegen =>
            Math.Max(0f, (_regen + _regenAdd) * _regenMult);

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

        public void ApplyRegenBase(float baseRegen)
        {
            _regen = baseRegen;
        }

        public void Damage(float amount)
        {
            if (amount <= 0) return;

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

        public void Recover(float amount)
        {
            if (amount <= 0) return;

            CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
            NotifyHp();
        }

        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            
            float sign = apply ? 1f : -1f;

            switch (cfg.stat)
            {
                case BuffStat.PlayerHpRegen:
                    ApplyRegenBuff(cfg, sign);
                    break;

                case BuffStat.PlayerShield:
                    ApplyShieldBuff(cfg, sign);
                    break;

                default:
                    ApplyHpBuff(cfg, sign);
                    break;
            }

            Clamp();
            NotifyHp();
            NotifyShield();
        }

        private void ApplyRegenBuff(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _regenAdd += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    _regenMult = sign > 0 ? _regenMult * cfg.value : _regenMult / cfg.value;
                    break;

                case BuffModType.Set:
                    if (sign > 0) _regen = cfg.value;
                    else _regen = 0f;
                    break;
            }
        }

        private void ApplyHpBuff(BuffSO cfg, float sign)
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
                    if (sign > 0) _baseHp = cfg.value;
                    break;
            }
        }

        private void ApplyShieldBuff(BuffSO cfg, float sign)
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
                    if (sign > 0) _baseShield = cfg.value;
                    break;
            }
        }

        private void Clamp()
        {
            CurrentHp = Math.Min(CurrentHp, MaxHp);
            CurrentShield = Math.Min(CurrentShield, MaxShield);
        }

        private void NotifyHp() =>
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        private void NotifyShield() =>
            OnShieldChanged?.Invoke(CurrentShield, MaxShield);
        
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
    }
}
