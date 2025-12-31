using System;
using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class EnergyStats : IEnergyStats
    {
        // -------------------------
        // BASE VALUES
        // -------------------------
        private float _baseMax;
        private float _baseRegen;

        // +ADD / *MULT bonus values
        private float _maxAdd = 0f;
        private float _maxMult = 1f;

        private float _regenAdd = 0f;
        private float _regenMult = 1f;

        private float _costMult = 1f; // 1 = normal

        // -------------------------
        // FINAL VALUES
        // -------------------------
        public float MaxEnergy => Math.Max(1f, (_baseMax + _maxAdd) * _maxMult);
        public float Regen     => Math.Max(0f, (_baseRegen + _regenAdd) * _regenMult);

        public float CurrentEnergy { get; private set; }
        public float CostMultiplier => _costMult;

        public event Action<float, float> OnEnergyChanged;

        // -------------------------
        // APPLY BASE
        // -------------------------
        public void ApplyBase(float max, float regen)
        {
            _baseMax = max;
            _baseRegen = regen;

            // Обновляем current без потери
            CurrentEnergy = Math.Clamp(CurrentEnergy, 0, MaxEnergy);

            Notify();
        }

        // -------------------------
        // BUFFS
        // -------------------------
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            float s = apply ? 1f : -1f;

            switch (cfg.stat)
            {
                case BuffStat.PlayerMaxEnergy:
                    ApplyMaxBuff(cfg, s);
                    break;

                case BuffStat.PlayerEnergyRegen:
                    ApplyRegenBuff(cfg, s);
                    break;

                case BuffStat.PlayerEnergyCostReduction:
                    ApplyCostBuff(cfg, s);
                    break;
            }

            CurrentEnergy = Math.Clamp(CurrentEnergy, 0f, MaxEnergy);
            Notify();
        }

        private void ApplyMaxBuff(BuffSO cfg, float s)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _maxAdd += cfg.value * s;
                    break;

                case BuffModType.Mult:
                    _maxMult = s > 0 ? _maxMult * cfg.value : _maxMult / cfg.value;
                    break;
            }
        }

        private void ApplyRegenBuff(BuffSO cfg, float s)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _regenAdd += cfg.value * s;
                    break;

                case BuffModType.Mult:
                    _regenMult = s > 0 ? _regenMult * cfg.value : _regenMult / cfg.value;
                    break;

                case BuffModType.Set:
                    if (s > 0) _baseRegen = cfg.value;
                    break;
            }
        }

        private void ApplyCostBuff(BuffSO cfg, float s)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _costMult += cfg.value * s;
                    break;

                case BuffModType.Mult:
                    _costMult = s > 0 ? _costMult * cfg.value : _costMult / cfg.value;
                    break;

                case BuffModType.Set:
                    if (s > 0) _costMult = cfg.value;
                    else _costMult = 1f;
                    break;
            }

            _costMult = Math.Clamp(_costMult, 0.1f, 10f);
        }

        // -------------------------
        // ENERGY ACTIONS
        // -------------------------
        public bool HasEnergy(float amount) => CurrentEnergy >= amount;

        public bool TrySpend(float amount)
        {
            if (CurrentEnergy < amount) return false;

            CurrentEnergy -= amount;
            Notify();
            return true;
        }

        public void Recover(float amount)
        {
            if (amount <= 0) return;

            CurrentEnergy = Math.Clamp(CurrentEnergy + amount, 0, MaxEnergy);
            Notify();
        }

        private void Notify() =>
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);

        public void SetCurrentEnergy(float value)
        {
            CurrentEnergy = Math.Clamp(value, 0, MaxEnergy);
            Notify();
        }

        public void SetMaxEnergyDirect(float max)
        {
            _baseMax = max;
            CurrentEnergy = Math.Clamp(CurrentEnergy, 0, MaxEnergy);
            Notify();
        }

        public void Reset()
        {
            _baseMax = 0f;
            _baseRegen = 0f;

            _maxAdd = 0f;
            _maxMult = 1f;

            _regenAdd = 0f;
            _regenMult = 1f;

            _costMult = 1f;

            CurrentEnergy = 0f;

            Notify();
        }
    }
}
