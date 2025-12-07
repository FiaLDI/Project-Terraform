using UnityEngine;

namespace Features.Camera.Domain
{
    public interface ICameraControlService
    {
        PlayerCameraState State { get; }

        void SetLookInput(Vector2 lookInput, float sensitivity, float deltaTime);
        void SwitchView();
        void UpdateTransition(float deltaTime);

        // FPS
        void UpdateRotationFPS(Transform body);

        // TPS
        void UpdateRotationTPS(Transform pivot, Transform body, float limit);

        // TPS distance & collision
        float ComputeTpsDistance(
            Vector3 pivotPos,
            Vector3 targetPos,
            LayerMask mask,
            float radius,
            float minDistance
        );

        Quaternion GetCameraRotation(Transform pivot);
        Vector3 GetCameraPosition(Transform pivot, float distance);

        void SetInputBlocked(bool blocked);
    }
}
