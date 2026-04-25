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
        private float _baseGravity;
        private float _baseJumpHeight;

        // ================= ADD =================
        private float _speedAdd;
        private float _walkAdd;
        private float _sprintAdd;
        private float _crouchAdd;
        private float _rotationAdd;
        private float _gravityAdd;
        private float _jumpAdd;

        // ================= MULT =================
        private float _speedMult = 1f;
        private float _walkMult = 1f;
        private float _sprintMult = 1f;
        private float _crouchMult = 1f;
        private float _rotationMult = 1f;
        private float _gravityMult = 1f;
        private float _jumpMult = 1f;

        // ================= FINAL =================
        public float BaseSpeed => (_baseSpeed + _speedAdd) * _speedMult;
        public float WalkSpeed => (_baseWalk + _walkAdd) * _walkMult;
        public float SprintSpeed => (_baseSprint + _sprintAdd) * _sprintMult;
        public float CrouchSpeed => (_baseCrouch + _crouchAdd) * _crouchMult;
        public float RotationSpeed => (_baseRotation + _rotationAdd) * _rotationMult;
        public float Gravity => (_baseGravity + _gravityAdd) * _gravityMult;
        public float JumpHeight => (_baseJumpHeight + _jumpAdd) * _jumpMult;

        // ================= BASE =================
        public void ApplyBase(
            float baseSpeed,
            float walk,
            float sprint,
            float crouch,
            float rotation,
            float gravity,
            float jumpHeight)
        {
            _baseSpeed = baseSpeed;
            _baseWalk = walk;
            _baseSprint = sprint;
            _baseCrouch = crouch;
            _baseRotation = rotation;
            _baseGravity = gravity;
            _baseJumpHeight = jumpHeight;
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
            if (key == StatKeys.Gravity)
            {
                _gravityAdd += value;
                return true;
            }
            if (key == StatKeys.JumpHeight)
            {
                _jumpAdd += value;
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
            if (key == StatKeys.Gravity)
            {
                _gravityMult *= value;
                return true;
            }
            if (key == StatKeys.JumpHeight)
            {
                _jumpMult *= value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _baseSpeed = 0f;
            _baseWalk = 0f;
            _baseSprint = 0f;
            _baseCrouch = 0f;
            _baseRotation = 0f;
            _baseGravity = 0f;
            _baseJumpHeight = 0f;

            _speedAdd = 0f;
            _walkAdd = 0f;
            _sprintAdd = 0f;
            _crouchAdd = 0f;
            _rotationAdd = 0f;
            _gravityAdd = 0f;
            _jumpAdd = 0f;

            _speedMult = 1f;
            _walkMult = 1f;
            _sprintMult = 1f;
            _crouchMult = 1f;
            _rotationMult = 1f;
            _gravityMult = 1f;
            _jumpMult = 1f;
        }

        public float Debug_BaseWalk => _baseWalk;
        public float Debug_AddWalk => _walkAdd;
        public float Debug_MultWalk => _walkMult;
    }
}
