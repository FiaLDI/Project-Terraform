using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class MovementStatsAdapter : MonoBehaviour
    {
        private IMovementStats _stats;

        // ===== CLIENT CACHE =====
        private float _walk;
        private float _sprint;
        private float _crouch;
        private float _rotation;
        private float _gravity;
        private float _jump;

        private bool _isReady;

        public bool IsReady => _stats != null || _isReady;

        // ===== READ API =====

        public float WalkSpeed => _stats != null ? _stats.WalkSpeed : _walk;
        public float SprintSpeed => _stats != null ? _stats.SprintSpeed : _sprint;
        public float CrouchSpeed => _stats != null ? _stats.CrouchSpeed : _crouch;
        public float RotationSpeed => _stats != null ? _stats.RotationSpeed : _rotation;

        public float Gravity => _stats != null ? _stats.Gravity : _gravity;
        public float JumpHeight => _stats != null ? _stats.JumpHeight : _jump;

        // ===== SERVER INIT =====

        public void Init(IMovementStats stats)
        {
            _stats = stats;
        }

        // ===== CLIENT SYNC =====

        public void Set(
            float walk,
            float sprint,
            float crouch,
            float rotation,
            float gravity,
            float jumpHeight)
        {
            _walk = walk;
            _sprint = sprint;
            _crouch = crouch;
            _rotation = rotation;

            _gravity = gravity;
            _jump = jumpHeight;

            _isReady = true;
        }

        // ===== SPEED HELPER =====

        public float GetSpeed(bool sprint, bool crouch)
        {
            if (!IsReady)
                return 0f;

            if (crouch) return CrouchSpeed;
            if (sprint) return SprintSpeed;
            return WalkSpeed;
        }
    }
}