using UnityEngine;

namespace Features.Camera.Domain
{
    public interface ICameraControlService
    {
        PlayerCameraState State { get; }

        void SetLookInput(Vector2 lookInput, float sensitivity, float deltaTime);
        void SwitchView();
        void UpdateTransition(float deltaTime);
        float ComputeTpsDistance(
            Vector3 pivotPos,
            Vector3 targetPos,
            LayerMask mask,
            float radius,
            float minDistance
        );

        void SetInputBlocked(bool blocked);
    }
}
