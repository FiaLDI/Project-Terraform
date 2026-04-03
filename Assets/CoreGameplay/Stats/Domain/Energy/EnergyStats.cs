using System;

namespace Features.Stats.Domain
{
    public class EnergyStats : IEnergyStats, IStatModifierTarget
    {
        public static readonly StatKey MaxEnergyKey = StatKeys.MaxEnergy;
        public static readonly StatKey RegenKey = StatKeys.EnergyRegen;
        public static readonly StatKey CostMultKey = StatKeys.EnergyCostMult;

        private float _baseMax;
        private float _baseRegen;

        private float _maxAdd;
        private float _maxMult = 1f;

        private float _regenAdd;
        private float _regenMult = 1f;

        private float _costMult = 1f;

        public float MaxEnergy => Math.Max(1f, (_baseMax + _maxAdd) * _maxMult);
        public float Regen => Math.Max(0f, (_baseRegen + _regenAdd) * _regenMult);

        public float CurrentEnergy { get; private set; }
        public float CostMultiplier => _costMult;

        public event Action<float, float> OnEnergyChanged;

        public void ApplyBase(float max, float regen)
        {
            _baseMax = max;
            _baseRegen = regen;
            CurrentEnergy = Math.Clamp(CurrentEnergy, 0, MaxEnergy);
            Notify();
        }

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id == MaxEnergyKey.Id) _maxAdd += value;
            else if (key.Id == RegenKey.Id) _regenAdd += value;
            else return false;

            Notify();
            return true;
        }

        public bool TryMultiply(StatKey key, float mult)
        {
            if (key.Id == MaxEnergyKey.Id) _maxMult *= mult;
            else if (key.Id == RegenKey.Id) _regenMult *= mult;
            else if (key.Id == CostMultKey.Id)
                _costMult = Math.Clamp(_costMult * mult, 0.1f, 10f);
            else return false;

            Notify();
            return true;
        }

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

        private void Notify() =>
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);

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

        public float Debug_BaseMax => _baseMax;
        public float Debug_AddMax => _maxAdd;
        public float Debug_MultMax => _maxMult;

        public float Debug_BaseRegen => _baseRegen;
        public float Debug_AddRegen => _regenAdd;
        public float Debug_MultRegen => _regenMult;
    }
}
