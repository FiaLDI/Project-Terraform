using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class MiningStats : IMiningStats
    {
        // База
        private float _basePower;

        // Баффы
        private float _addBonus = 0f;
        private float _multBonus = 1f;

        // Финальное значение
        public float MiningPower { get; private set; }

        public void ApplyBase(float pwr)
        {
            _basePower = pwr;
            Recalc();
        }

        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            if (cfg == null) return;

            float sign = apply ? 1f : -1f;

            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _addBonus += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    if (apply)
                        _multBonus *= cfg.value;
                    else if (cfg.value != 0f)
                        _multBonus /= cfg.value;
                    break;

                case BuffModType.Set:
                    _multBonus = apply ? cfg.value : 1f;
                    break;
            }

            Recalc();
        }

        private void Recalc()
        {
            MiningPower = (_basePower + _addBonus) * _multBonus;
            if (MiningPower < 0f)
                MiningPower = 0f;
        }
        public void Reset()
        {
            _basePower = 1f;
            _addBonus = 0f;
            _multBonus = 1f;
        }
    }
}
