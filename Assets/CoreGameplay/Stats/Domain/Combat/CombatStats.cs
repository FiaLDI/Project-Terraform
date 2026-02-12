using UnityEngine;

namespace Features.Stats.Domain
{
    public class CombatStats : ICombatStats, IStatModifierTarget
    {
        // =========================
        // STAT KEYS
        // =========================

        private static readonly StatKey DamageKey = StatKeys.DamageMultiplier;
        private static readonly StatKey FireRateKey = StatKeys.FireRate;
        private static readonly StatKey SpreadKey = StatKeys.Spread;
        private static readonly StatKey AimSpreadKey = StatKeys.AimSpread;
        private static readonly StatKey RangeKey = StatKeys.Range;
        private static readonly StatKey RecoilKey = StatKeys.Recoil;
        private static readonly StatKey MagazineKey = StatKeys.MagazineSize;

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

        // =========================
        // ADD
        // =========================

        private float _addDamage;
        private float _addFireRate;
        private float _addSpread;
        private float _addAimSpread;
        private float _addRange;
        private float _addRecoil;
        private int _addMagazine;

        // =========================
        // MULT
        // =========================

        private float _multDamage = 1f;
        private float _multFireRate = 1f;
        private float _multSpread = 1f;
        private float _multAimSpread = 1f;
        private float _multRange = 1f;
        private float _multRecoil = 1f;

        // =========================
        // PROPERTIES
        // =========================

        public float DamageMultiplier => (_baseDamage + _addDamage) * _multDamage;
        public float FireRate => (_baseFireRate + _addFireRate) * _multFireRate;
        public float Spread => (_baseSpread + _addSpread) * _multSpread;
        public float AimSpread => (_baseAimSpread + _addAimSpread) * _multAimSpread;
        public float Range => (_baseRange + _addRange) * _multRange;
        public float Recoil => (_baseRecoil + _addRecoil) * _multRecoil;
        public int MagazineSize => Mathf.Max(0, _baseMagazine + _addMagazine);

        // =========================
        // APPLY BASE
        // =========================

        public void ApplyBase(
            float damageMultiplier,
            float fireRate,
            float spread,
            float aimSpread,
            float range,
            float recoil,
            int magazineSize)
        {
            _baseDamage = damageMultiplier;
            _baseFireRate = fireRate;
            _baseSpread = spread;
            _baseAimSpread = aimSpread;
            _baseRange = range;
            _baseRecoil = recoil;
            _baseMagazine = magazineSize;
        }

        // =========================
        // MODIFIERS
        // =========================

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id == DamageKey.Id) { _addDamage += value; return true; }
            if (key.Id == FireRateKey.Id) { _addFireRate += value; return true; }
            if (key.Id == SpreadKey.Id) { _addSpread += value; return true; }
            if (key.Id == AimSpreadKey.Id) { _addAimSpread += value; return true; }
            if (key.Id == RangeKey.Id) { _addRange += value; return true; }
            if (key.Id == RecoilKey.Id) { _addRecoil += value; return true; }
            if (key.Id == MagazineKey.Id) { _addMagazine += Mathf.RoundToInt(value); return true; }

            return false;
        }

        public bool TryMultiply(StatKey key, float multiplier)
        {
            if (key.Id == DamageKey.Id) { _multDamage *= multiplier; return true; }
            if (key.Id == FireRateKey.Id) { _multFireRate *= multiplier; return true; }
            if (key.Id == SpreadKey.Id) { _multSpread *= multiplier; return true; }
            if (key.Id == AimSpreadKey.Id) { _multAimSpread *= multiplier; return true; }
            if (key.Id == RangeKey.Id) { _multRange *= multiplier; return true; }
            if (key.Id == RecoilKey.Id) { _multRecoil *= multiplier; return true; }

            return false;
        }

        // =========================
        // RESET
        // =========================

        public void Reset()
        {
            _baseDamage = 0f;
            _baseFireRate = 0f;
            _baseSpread = 0f;
            _baseAimSpread = 0f;
            _baseRange = 0f;
            _baseRecoil = 0f;
            _baseMagazine = 0;

            _addDamage = 0f;
            _addFireRate = 0f;
            _addSpread = 0f;
            _addAimSpread = 0f;
            _addRange = 0f;
            _addRecoil = 0f;
            _addMagazine = 0;

            _multDamage = 1f;
            _multFireRate = 1f;
            _multSpread = 1f;
            _multAimSpread = 1f;
            _multRange = 1f;
            _multRecoil = 1f;
        }
    }
}
