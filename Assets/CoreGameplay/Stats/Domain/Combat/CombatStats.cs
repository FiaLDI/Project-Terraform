namespace Features.Stats.Domain
{
    public class CombatStats : ICombatStats, IStatModifierTarget
    {
        public static readonly StatKey DamageKey = StatKeys.DamageMultiplier;

        private float _base;
        private float _add;
        private float _mult = 1f;

        public float DamageMultiplier => (_base + _add) * _mult;

        public void ApplyBase(float dmg) => _base = dmg;

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id != DamageKey.Id) return false;
            _add += value;
            return true;
        }

        public bool TryMultiply(StatKey key, float multiplier)
        {
            if (key.Id != DamageKey.Id) return false;
            _mult *= multiplier;
            return true;
        }

        public void Reset()
        {
            _base = 0f;
            _add = 0f;
            _mult = 1f;
        }
    }
}
