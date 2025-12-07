using UnityEngine;
using Features.Camera.Domain;

namespace Features.Camera.Application
{
    public class CameraControlService : ICameraControlService
    {
        private float _yaw;
        private float _pitch;

        private float _blend; // 0 = FPS, 1 = TPS
        private bool _tpsMode;

        private float _tpsDistance = 3f;
        private float _transitionSpeed = 6f;

        private bool _inputBlocked;

        public PlayerCameraState State
        {
            get
            {
                return new PlayerCameraState
                {
                    Yaw = _yaw,
                    Pitch = _pitch,
                    Blend = _blend,
                    TpsDistance = _tpsDistance
                };
            }
        }

        // -----------------------------------------------------
        // INPUT
        // -----------------------------------------------------
        public void SetLookInput(Vector2 lookInput, float sensitivity, float deltaTime)
        {
            if (_inputBlocked) return;

            _yaw += lookInput.x * sensitivity * deltaTime;
            _pitch -= lookInput.y * sensitivity * deltaTime;

            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
        }

        public void SwitchView()
        {
            _tpsMode = !_tpsMode;
        }

        public void UpdateTransition(float deltaTime)
        {
            float target = _tpsMode ? 1f : 0f;
            _blend = Mathf.Lerp(_blend, target, deltaTime * _transitionSpeed);
        }

        // -----------------------------------------------------
        // FPS
        // -----------------------------------------------------
        public void UpdateRotationFPS(Transform body)
        {
            if (body == null) return;
            body.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        // -----------------------------------------------------
        // TPS ROTATION
        // -----------------------------------------------------
        public void UpdateRotationTPS(Transform pivot, Transform body, float limit)
        {
            if (pivot == null || body == null) return;

            pivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            body.rotation = Quaternion.Euler(0f, _yaw, 0f);

            // Auto-align body within angular limit
            Vector3 forward = body.forward;
            Vector3 camForward = pivot.forward;

            forward.y = 0;
            camForward.y = 0;

            float angle = Vector3.SignedAngle(forward, camForward, Vector3.up);

            if (Mathf.Abs(angle) > limit)
            {
                float delta = Mathf.Sign(angle) * (Mathf.Abs(angle) - limit);
                body.Rotate(Vector3.up * delta);
            }
        }

        // -----------------------------------------------------
        // TPS COLLISION
        // -----------------------------------------------------
        public float ComputeTpsDistance(
            Vector3 pivotPos,
            Vector3 targetPosition,
            LayerMask mask,
            float radius,
            float minDistance)
        {
            Vector3 dir = (targetPosition - pivotPos).normalized;
            float targetDistance = Vector3.Distance(pivotPos, targetPosition);

            if (Physics.SphereCast(pivotPos, radius, dir, out RaycastHit hit, targetDistance, mask))
                return Mathf.Max(hit.distance - 0.05f, minDistance);

            return targetDistance;
        }

        // -----------------------------------------------------
        // CAMERA OUTPUT
        // -----------------------------------------------------
        public Quaternion GetCameraRotation(Transform pivot)
        {
            return pivot != null ? pivot.rotation : Quaternion.identity;
        }

        public Vector3 GetCameraPosition(Transform pivot, float distance)
        {
            if (pivot == null) return Vector3.zero;
            return pivot.position - pivot.forward * distance;
        }

        // -----------------------------------------------------
        public void SetInputBlocked(bool blocked)
        {
            _inputBlocked = blocked;
        }
    }
}
