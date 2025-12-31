using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class CombatStats : ICombatStats
    {
        // BASE
        private float _baseDamage;

        // BUFF MODIFIERS
        private float _bonusAdd = 0f;
        private float _bonusMult = 1f;
        private float _bonusSet = -1f; // -1 = не используется

        // PUBLIC FINAL STAT
        public float DamageMultiplier { get; private set; }

        // -----------------------------------
        // BASE VALUES
        // -----------------------------------
        public void ApplyBase(float dmg)
        {
            _baseDamage = dmg;
            Recalc();
        }

        // -----------------------------------
        // BUFFS
        // -----------------------------------
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            float sign = apply ? 1f : -1f;

            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _bonusAdd += cfg.value * sign;
                    break;

                case BuffModType.Mult:
                    if (apply) _bonusMult *= cfg.value;
                    else        _bonusMult /= cfg.value;
                    break;

                case BuffModType.Set:
                    if (apply)
                        _bonusSet = cfg.value;
                    else
                        _bonusSet = -1f;
                    break;
            }

            Recalc();
        }

        // -----------------------------------
        // RECALC FINAL DAMAGE OUTPUT
        // -----------------------------------
        private void Recalc()
        {
            if (_bonusSet >= 0)
            {
                DamageMultiplier = _bonusSet;
                return;
            }

            DamageMultiplier =
                (_baseDamage + _bonusAdd) * _bonusMult;
        }

        public void Reset()
        {
            
        }
    }
}
