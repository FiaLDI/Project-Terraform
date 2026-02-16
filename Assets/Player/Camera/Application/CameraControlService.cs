using UnityEngine;
using Features.Camera.Domain;

namespace Features.Camera.Application
{
    public class CameraControlService : ICameraControlService
    {
        private float _yaw;
        private float _pitch;

        private float _blend;
        private bool _tpsMode;

        private float _tpsDistance = 3f;
        private float _transitionSpeed = 6f;

        private bool _inputBlocked;

        public PlayerCameraState State => new PlayerCameraState
        {
            Yaw = _yaw,
            Pitch = _pitch,
            Blend = _blend,
            TpsDistance = _tpsDistance
        };

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

        public float ComputeTpsDistance(
            Vector3 pivotPos,
            Vector3 targetPos,
            LayerMask mask,
            float radius,
            float minDistance)
        {
            Vector3 dir = (targetPos - pivotPos).normalized;
            float dist = Vector3.Distance(pivotPos, targetPos);

            if (Physics.SphereCast(pivotPos, radius, dir, out RaycastHit hit, dist, mask))
                return Mathf.Max(hit.distance - 0.05f, minDistance);

            return dist;
        }

        public void SetInputBlocked(bool blocked)
        {
            _inputBlocked = blocked;
        }
    }
}
