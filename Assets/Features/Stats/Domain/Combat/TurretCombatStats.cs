using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class TurretCombatStats : CombatStats, ITurretCombatStats
    {
        private float _baseFireRate = 1f;
        private float _fireRateAdd = 0f;
        private float _fireRateMult = 1f;

        public float FireRate =>
            (_baseFireRate + _fireRateAdd) * _fireRateMult;

        public void ApplyFireRateBase(float baseRate)
        {
            _baseFireRate = baseRate;
        }

        public void ApplyFireRateBuff(BuffSO cfg, bool apply)
        {
            if (cfg.stat != BuffStat.TurretFireRate)
                return;

            float sign = apply ? 1f : -1f;

            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _fireRateAdd += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    _fireRateMult = apply ? _fireRateMult * cfg.value : _fireRateMult / cfg.value;
                    break;

                case BuffModType.Set:
                    if (apply) _baseFireRate = cfg.value;
                    break;
            }
        }
    }
}
