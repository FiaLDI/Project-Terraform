namespace Features.Stats.Domain
{
    public class TurretCombatStats
        : CombatStats, ITurretCombatStats, IStatModifierTarget
    {
        public static readonly StatKey FireRateKey = StatKeys.FireRate;

        private float _baseFireRate = 1f;
        private float _add;
        private float _mult = 1f;

        public float FireRate => (_baseFireRate + _add) * _mult;

        public void ApplyFireRateBase(float baseRate)
        {
            _baseFireRate = baseRate;
        }

        public bool TryAdd(StatKey key, float value)
        {
            if (base.TryAdd(key, value)) return true;
            if (key.Id != FireRateKey.Id) return false;

            _add += value;
            return true;
        }

        public bool TryMultiply(StatKey key, float multiplier)
        {
            if (base.TryMultiply(key, multiplier)) return true;
            if (key.Id != FireRateKey.Id) return false;

            _mult *= multiplier;
            return true;
        }

        public new void Reset()
        {
            base.Reset();
            _baseFireRate = 1f;
            _add = 0f;
            _mult = 1f;
        }
    }
}
