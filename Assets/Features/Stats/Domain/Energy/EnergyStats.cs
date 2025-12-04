using System;
using Features.Buffs.Domain;
using UnityEngine;   // для Mathf.Clamp

namespace Features.Stats.Domain
{
    public class EnergyStats : IEnergyStats
    {
        public float MaxEnergy { get; private set; }
        public float CurrentEnergy { get; private set; }
        public float Regen { get; private set; }

        // коэффициент стоимости (1 = базовая, 0.8 = -20% cost, 1.2 = +20% cost)
        private float _costMult = 1f;
        public float CostMultiplier => _costMult;

        public event Action<float, float> OnEnergyChanged;

        // ------------------------------------------
        // BASE
        // ------------------------------------------
        public void ApplyBase(float max, float regen)
        {
            MaxEnergy = max;
            Regen = regen;
            CurrentEnergy = MaxEnergy;
            _costMult = 1f; // по умолчанию без модификаторов

            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
        }

        // ------------------------------------------
        // BUFFS
        // ------------------------------------------
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            if (cfg == null) return;

            float sign = apply ? 1f : -1f;

            switch (cfg.stat)
            {
                case BuffStat.PlayerMaxEnergy:
                    ApplyMaxEnergyBuff(cfg, sign);
                    break;

                case BuffStat.PlayerEnergyRegen:
                    ApplyRegenBuff(cfg, sign);
                    break;

                case BuffStat.PlayerEnergyCostReduction:
                    ApplyCostBuff(cfg, sign);
                    break;
            }

            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, MaxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
        }

        private void ApplyMaxEnergyBuff(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    MaxEnergy += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    if (sign > 0) MaxEnergy *= cfg.value;
                    else MaxEnergy /= cfg.value;
                    break;

                case BuffModType.Set:
                    if (sign > 0) MaxEnergy = cfg.value;
                    // при снятии Set откатывать не будем (обычно не надо),
                    // при необходимости — можно держать базу отдельно.
                    break;
            }
        }

        private void ApplyRegenBuff(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    Regen += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    if (sign > 0) Regen *= cfg.value;
                    else Regen /= cfg.value;
                    break;

                case BuffModType.Set:
                    if (sign > 0) Regen = cfg.value;
                    break;
            }
        }

        private void ApplyCostBuff(BuffSO cfg, float sign)
        {
            switch (cfg.modType)
            {
                case BuffModType.Add:
                    // Add — всегда процент -> value = 0.2 = +20%, value = -0.2 = -20%
                    _costMult += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    if (sign > 0) 
                        _costMult *= cfg.value;   // value < 1  → уменьшение стоимости
                    else 
                        _costMult /= cfg.value;
                    break;

                case BuffModType.Set:
                    if (sign > 0) _costMult = cfg.value;
                    else _costMult = 1f;
                    break;
            }

            // Clamp чтобы не улететь
            _costMult = Mathf.Clamp(_costMult, 0.1f, 10f);
        }


        // ------------------------------------------
        // SPEND / CHECK / RECOVER
        // ------------------------------------------
        public bool HasEnergy(float amount) => CurrentEnergy >= amount;

        public bool TrySpend(float amount)
        {
            if (!HasEnergy(amount)) return false;

            CurrentEnergy -= amount;
            OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
            return true;
        }

        public void Recover(float amount)
        {
            if (amount <= 0f) return;

            float newValue = Math.Clamp(CurrentEnergy + amount, 0, MaxEnergy);
            if (!Mathf.Approximately(newValue, CurrentEnergy))
            {
                CurrentEnergy = newValue;
                OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
            }
        }
    }
}
