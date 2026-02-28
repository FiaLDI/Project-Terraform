using Features.Stats.Domain;

namespace Features.Stats.Domain
{
    public sealed class MovementStats : IMovementStats, IStatModifierTarget
    {
        // ================= BASE =================
        private float _baseSpeed;
        private float _baseWalk;
        private float _baseSprint;
        private float _baseCrouch;
        private float _baseRotation;

        // ================= ADD =================
        private float _speedAdd;
        private float _walkAdd;
        private float _sprintAdd;
        private float _crouchAdd;
        private float _rotationAdd;

        // ================= MULT =================
        private float _speedMult = 1f;
        private float _walkMult = 1f;
        private float _sprintMult = 1f;
        private float _crouchMult = 1f;
        private float _rotationMult = 1f;

        // ================= FINAL =================
        public float BaseSpeed => (_baseSpeed + _speedAdd) * _speedMult;
        public float WalkSpeed => (_baseWalk + _walkAdd) * _walkMult;
        public float SprintSpeed => (_baseSprint + _sprintAdd) * _sprintMult;
        public float CrouchSpeed => (_baseCrouch + _crouchAdd) * _crouchMult;
        public float RotationSpeed => (_baseRotation + _rotationAdd) * _rotationMult;

        // ================= BASE =================
        public void ApplyBase(
            float baseSpeed,
            float walk,
            float sprint,
            float crouch,
            float rotation)
        {
            _baseSpeed = baseSpeed;
            _baseWalk = walk;
            _baseSprint = sprint;
            _baseCrouch = crouch;
            _baseRotation = rotation;
        }

        // ================= MODIFIERS =================
        public bool TryAdd(StatKey key, float value)
        {
            if (key == StatKeys.MoveSpeed)
            {
                _speedAdd += value;
                return true;
            }
            if (key == StatKeys.WalkSpeed)
            {
                _walkAdd += value;
                return true;
            }
            if (key == StatKeys.SprintSpeed)
            {
                _sprintAdd += value;
                return true;
            }
            if (key == StatKeys.CrouchSpeed)
            {
                _crouchAdd += value;
                return true;
            }
            if (key == StatKeys.RotationSpeed)
            {
                _rotationAdd += value;
                return true;
            }

            return false;
        }

        public bool TryMultiply(StatKey key, float value)
        {
            if (key == StatKeys.MoveSpeed)
            {
                _speedMult *= value;
                return true;
            }
            if (key == StatKeys.WalkSpeed)
            {
                _walkMult *= value;
                return true;
            }
            if (key == StatKeys.SprintSpeed)
            {
                _sprintMult *= value;
                return true;
            }
            if (key == StatKeys.CrouchSpeed)
            {
                _crouchMult *= value;
                return true;
            }
            if (key == StatKeys.RotationSpeed)
            {
                _rotationMult *= value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _speedAdd = _walkAdd = _sprintAdd = _crouchAdd = _rotationAdd = 0f;
            _speedMult = _walkMult = _sprintMult = _crouchMult = _rotationMult = 1f;
        }
    }
}
