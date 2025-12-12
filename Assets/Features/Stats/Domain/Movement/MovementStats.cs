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
        // BUFF MODIFIERS (ADD & MULT)
        // =============================
        private float _speedAdd = 0f;
        private float _speedMult = 1f;

        private float _walkAdd = 0f;
        private float _walkMult = 1f;

        private float _sprintAdd = 0f;
        private float _sprintMult = 1f;

        private float _crouchAdd = 0f;
        private float _crouchMult = 1f;

        private float _baseRotation;
        private float _rotationAdd = 0f;
        private float _rotationMult = 1f;

        // =============================
        // FINAL VALUES
        // =============================
        public float BaseSpeed { get; private set; }
        public float WalkSpeed { get; private set; }
        public float SprintSpeed { get; private set; }
        public float CrouchSpeed { get; private set; }
        public float RotationSpeed { get; private set; }

        // =============================
        // APPLY BASE
        // =============================
        public void ApplyBase(float baseSpeed, float walk, float sprint, float crouch, float rotation)
        {
            _baseSpeed = baseSpeed;
            _baseWalk = walk;
            _baseSprint = sprint;
            _baseCrouch = crouch;
            _baseRotation = rotation;

            Recalc();
        }

        // =============================
        // APPLY BUFF
        // =============================
        public void ApplyBuff(BuffSO cfg, bool apply)
        {
            if (cfg == null) return;

            float sign = apply ? 1f : -1f;

            switch (cfg.stat)
            {
                // ---------------------------------
                // BASE SPEED
                // ---------------------------------
                case BuffStat.PlayerMoveSpeed:
                    _speedAdd += sign * cfg.value;
                    break;

                case BuffStat.PlayerMoveSpeedMult:
                    if (apply) _speedMult *= cfg.value;
                    else _speedMult /= cfg.value;
                    break;

                // ---------------------------------
                // WALK
                // ---------------------------------
                case BuffStat.PlayerWalkSpeed:
                    _walkAdd += sign * cfg.value;
                    break;

                case BuffStat.PlayerWalkSpeedMult:
                    if (apply) _walkMult *= cfg.value;
                    else _walkMult /= cfg.value;
                    break;

                // ---------------------------------
                // SPRINT
                // ---------------------------------
                case BuffStat.PlayerSprintSpeed:
                    _sprintAdd += sign * cfg.value;
                    break;

                case BuffStat.PlayerSprintSpeedMult:
                    if (apply) _sprintMult *= cfg.value;
                    else _sprintMult /= cfg.value;
                    break;

                // ---------------------------------
                // CROUCH
                // ---------------------------------
                case BuffStat.PlayerCrouchSpeed:
                    _crouchAdd += sign * cfg.value;
                    break;

                case BuffStat.PlayerCrouchSpeedMult:
                    if (apply) _crouchMult *= cfg.value;
                    else _crouchMult /= cfg.value;
                    break;
                
                case BuffStat.RotationSpeed:
                    _rotationAdd += sign * cfg.value;
                    break;

                case BuffStat.RotationSpeedMult:
                    if (apply) _rotationMult *= cfg.value;
                    else _rotationMult /= cfg.value;
                    break;
            }

            Recalc();
        }

        // =============================
        // RECALC FINAL VALUES
        // =============================
        private void Recalc()
        {
            BaseSpeed = (_baseSpeed + _speedAdd) * _speedMult;
            WalkSpeed = (_baseWalk + _walkAdd) * _walkMult;
            SprintSpeed = (_baseSprint + _sprintAdd) * _sprintMult;
            CrouchSpeed = (_baseCrouch + _crouchAdd) * _crouchMult;
            RotationSpeed  = (_baseRotation + _rotationAdd) * _rotationMult;
        }
    }
}
