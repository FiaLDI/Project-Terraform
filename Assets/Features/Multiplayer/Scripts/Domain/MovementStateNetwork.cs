using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerAnimationController))]
    public sealed class MovementStateNetwork : NetworkBehaviour
    {
        // ================= SYNC =================
        private readonly SyncVar<float> _planarSpeed = new();
        private readonly SyncVar<bool> _isGrounded = new();
        private readonly SyncVar<bool> _isCrouching = new();

        private const float SPEED_EPSILON = 0.05f;

        // ================= CLIENT SMOOTHING =================
        private float _targetSpeed;
        private float _currentSpeed;

        private PlayerAnimationController anim;

        // ================= LIFECYCLE =================

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            anim = GetComponent<PlayerAnimationController>();

            _planarSpeed.OnChange += OnSpeedChanged;
            _isGrounded.OnChange += OnGroundedChanged;
            _isCrouching.OnChange += OnCrouchChanged;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            _planarSpeed.OnChange -= OnSpeedChanged;
            _isGrounded.OnChange -= OnGroundedChanged;
            _isCrouching.OnChange -= OnCrouchChanged;
        }

        // ================= SERVER API =================

        public void SetMovementState(float speed, bool grounded, bool crouching)
        {
            if (!IsServerInitialized)
                return;

            // 🔑 не спамим сеть
            if (Mathf.Abs(_planarSpeed.Value - speed) > SPEED_EPSILON)
                _planarSpeed.Value = speed;

            if (_isGrounded.Value != grounded)
                _isGrounded.Value = grounded;

            if (_isCrouching.Value != crouching)
                _isCrouching.Value = crouching;
        }

        // ================= CLIENT RECEIVE =================

        private void OnSpeedChanged(float oldVal, float newVal, bool asServer)
        {
            // ❗ только сохраняем цель, НЕ трогаем Animator
            _targetSpeed = newVal;
        }

        private void OnGroundedChanged(bool oldVal, bool newVal, bool asServer)
        {
            anim?.SetGrounded(newVal);
        }

        private void OnCrouchChanged(bool oldVal, bool newVal, bool asServer)
        {
            anim?.SetCrouch(newVal);
        }

        // ================= CLIENT SMOOTH UPDATE =================

        private void Update()
        {
            if (anim == null)
                return;

            _currentSpeed = Mathf.Lerp(
                _currentSpeed,
                _targetSpeed,
                Time.deltaTime * 8f
            );

            float animSpeed = _currentSpeed < 0.1f ? 0f : _currentSpeed;

            anim.SetSpeed(animSpeed);
        }
    }
}
