using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Features.Stats.Domain
{
    public class CombatStats : ICombatStats, IStatModifierTarget
    {
        // =========================
        // BASE
        // =========================

        private float _baseDamage;
        private float _baseFireRate;
        private float _baseSpread;
        private float _baseAimSpread;
        private float _baseRange;
        private float _baseRecoil;
        private int _baseMagazine;
        private float _baseCritChance;
        private float _baseCritMultiplier;
        private float _basePenetration;

        // =========================
        // ADD (flat bonuses)
        // =========================

        private float _flatDamage;
        private float _addFireRate;
        private float _addSpread;
        private float _addAimSpread;
        private float _addRange;
        private float _addRecoil;
        private int _addMagazine;
        private float _addCritChance;
        private float _addCritMultiplier;
        private float _addPenetration;

        // =========================
        // MULTIPLIERS
        // =========================

        private float _damageMultiplier = 1f;
        private float _multFireRate = 1f;
        private float _multSpread = 1f;
        private float _multAimSpread = 1f;
        private float _multRange = 1f;
        private float _multRecoil = 1f;
        private float _multCritChance = 0.1f;
        private float _multCritMultiplier = 1f;
        private float _multPenetration = 1f;

        // =========================
        // PROPERTIES
        // =========================

        public float BaseDamage => _baseDamage;
        public float DamageMultiplier => _damageMultiplier;

        public float FinalDamage =>
            (_baseDamage + _flatDamage) * _damageMultiplier;

        public float FireRate =>
            (_baseFireRate + _addFireRate) * _multFireRate;

        public float Spread =>
            (_baseSpread + _addSpread) * _multSpread;

        public float AimSpread =>
            (_baseAimSpread + _addAimSpread) * _multAimSpread;

        public float Range =>
            (_baseRange + _addRange) * _multRange;

        public float Recoil =>
            (_baseRecoil + _addRecoil) * _multRecoil;

        public int MagazineSize =>
            Mathf.Max(0, _baseMagazine + _addMagazine);

        public float CritChance =>
            (_baseCritChance + _addCritChance) * _multCritChance;

        public float CritMultiplier =>
            (_baseCritMultiplier + _addCritMultiplier) * _multCritMultiplier;

        public float Penetration =>
            (_basePenetration + _addPenetration) * _multPenetration;

        // =========================
        // APPLY BASE
        // =========================

        public void ApplyBase(
            float baseDamage,
            float fireRate,
            float spread,
            float aimSpread,
            float range,
            float recoil,
            int magazineSize,
            float critChance,
            float critMultiplier,
            float penetration)
        {
            _baseDamage = baseDamage;
            _baseFireRate = fireRate;
            _baseSpread = spread;
            _baseAimSpread = aimSpread;
            _baseRange = range;
            _baseRecoil = recoil;
            _baseMagazine = magazineSize;
            _baseCritChance = critChance;
            _baseCritMultiplier = critMultiplier;
            _basePenetration = penetration;
        }

        // =========================
        // MODIFIERS
        // =========================

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id == StatKeys.FlatDamage.Id)
            {
                _flatDamage += value;
                return true;
            }

            if (key.Id == StatKeys.FireRate.Id)
            {
                _addFireRate += value;
                return true;
            }

            if (key.Id == StatKeys.Spread.Id)
            {
                _addSpread += value;
                return true;
            }

            if (key.Id == StatKeys.AimSpread.Id)
            {
                _addAimSpread += value;
                return true;
            }

            if (key.Id == StatKeys.Range.Id)
            {
                _addRange += value;
                return true;
            }

            if (key.Id == StatKeys.Recoil.Id)
            {
                _addRecoil += value;
                return true;
            }

            if (key.Id == StatKeys.MagazineSize.Id)
            {
                _addMagazine += Mathf.RoundToInt(value);
                return true;
            }

            if (key.Id == StatKeys.CritChance.Id)
            {
                _addCritChance += value;
                return true;
            }

            if (key.Id == StatKeys.CritMultiplier.Id)
            {
                _addCritMultiplier += value;
                return true;
            }

            if (key.Id == StatKeys.Penetration.Id)
            {
                _addPenetration += value;
                return true;
            }

            return false;
        }

        public bool TryMultiply(StatKey key, float multiplier)
        {
            if (key.Id == StatKeys.DamageMultiplier.Id)
            {
                _damageMultiplier *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.FireRate.Id)
            {
                _multFireRate *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.Spread.Id)
            {
                _multSpread *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.AimSpread.Id)
            {
                _multAimSpread *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.Range.Id)
            {
                _multRange *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.Recoil.Id)
            {
                _multRecoil *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.CritChance.Id)
            {
                _multCritChance *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.CritMultiplier.Id)
            {
                _multCritMultiplier *= multiplier;
                return true;
            }

            if (key.Id == StatKeys.Penetration.Id)
            {
                _multPenetration *= multiplier;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _baseDamage = 0f;
            _baseFireRate = 0f;
            _baseSpread = 0f;
            _baseAimSpread = 0f;
            _baseRange = 0f;
            _baseRecoil = 0f;
            _baseMagazine = 0;
            _baseCritChance = 0f;
            _baseCritMultiplier = 0f;
            _basePenetration = 0f;

            _flatDamage = 0f;
            _addFireRate = 0f;
            _addSpread = 0f;
            _addAimSpread = 0f;
            _addRange = 0f;
            _addRecoil = 0f;
            _addMagazine = 0;
            _addCritChance = 0;
            _addCritMultiplier = 0;
            _addPenetration = 0f;

            _damageMultiplier = 1f;
            _multFireRate = 1f;
            _multSpread = 1f;
            _multAimSpread = 1f;
            _multRange = 1f;
            _multRecoil = 1f;
            _multCritChance = 1f;
            _multCritMultiplier = 1f;
            _multPenetration = 1f;
        }

        public float Debug_BaseDamage => _baseDamage;
        public float Debug_AddDamage => _flatDamage;
        public float Debug_MultDamage => _damageMultiplier;
        public float Debug_BaseFireRate => _baseFireRate;
        public float Debug_AddFireRate => _addFireRate;
        public float Debug_MultFireRate => _multFireRate;
        // CRIT
        public float Debug_BaseCritChance => _baseCritChance;
        public float Debug_AddCritChance => _addCritChance;
        public float Debug_MultCritChance => _multCritChance;

        public float Debug_BaseCritMultiplier => _baseCritMultiplier;
        public float Debug_AddCritMultiplier => _addCritMultiplier;
        public float Debug_MultCritMultiplier => _multCritMultiplier;

        // PENETRATION
        public float Debug_BasePenetration => _basePenetration;
        public float Debug_AddPenetration => _addPenetration;
        public float Debug_MultPenetration => _multPenetration;
    }
}