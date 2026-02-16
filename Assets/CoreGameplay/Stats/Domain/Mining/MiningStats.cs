namespace Features.Stats.Domain
{
    public sealed class MiningStats : IMiningStats, IStatModifierTarget
    {
        public static readonly StatKey MiningPowerKey = StatKeys.MiningPower;

        private float _base;
        private float _add;
        private float _mult = 1f;

        public float MiningPower => (_base + _add) * _mult;

        public void ApplyBase(float power) => _base = power;

        public bool TryAdd(StatKey key, float value)
        {
            if (key.Id != MiningPowerKey.Id) return false;
            _add += value;
            return true;
        }

        public bool TryMultiply(StatKey key, float mult)
        {
            if (key.Id != MiningPowerKey.Id) return false;
            _mult *= mult;
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
