using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public class MovementStats : IMovementStats
    {
        // =============================
        // BASE VALUES
        // =============================
        private float _baseSpeed;
        private float _baseWalk;
        private float _baseSprint;
        private float _baseCrouch;

        // =============================
        // BUFF MODIFIERS
        // =============================
        private float _addBonus = 0f;
        private float _multBonus = 1f;

        // =============================
        // FINAL VALUES (CALCULATED)
        // =============================
        public float BaseSpeed { get; private set; }
        public float WalkSpeed { get; private set; }
        public float SprintSpeed { get; private set; }
        public float CrouchSpeed { get; private set; }

        // =============================
        // APPLY BASE STATS
        // =============================
        public void ApplyBase(float baseSpeed, float walk, float sprint, float crouch)
        {
            _baseSpeed  = baseSpeed;
            _baseWalk   = walk;
            _baseSprint = sprint;
            _baseCrouch = crouch;

            Recalc();
        }

        // =============================
        // APPLY BUFFS
        // =============================
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            if (cfg == null) return;

            float sign = apply ? 1f : -1f;

            switch (cfg.modType)
            {
                case BuffModType.Add:
                    _addBonus += sign * cfg.value;
                    break;

                case BuffModType.Mult:
                    if (apply)  _multBonus *= cfg.value;
                    else        _multBonus /= cfg.value;
                    break;

                case BuffModType.Set:
                    // SET → заменяем всю скорость
                    if (apply)
                    {
                        BaseSpeed  = cfg.value;
                        WalkSpeed  = cfg.value * 0.8f;
                        SprintSpeed = cfg.value * 1.5f;
                        CrouchSpeed = cfg.value * 0.5f;
                        return;
                    }
                    else
                    {
                        // снимаем Set → возвращаем оригинальные
                        _addBonus = 0f;
                        _multBonus = 1f;
                    }
                    break;
            }

            Recalc();
        }

        // =============================
        // RECALCULATE FINAL VALUES
        // =============================
        private void Recalc()
        {
            float final = (_baseSpeed + _addBonus) * _multBonus;

            BaseSpeed   = final;
            WalkSpeed   = final * 0.8f;
            SprintSpeed = final * 1.5f;
            CrouchSpeed = final * 0.5f;
        }
    }
}
