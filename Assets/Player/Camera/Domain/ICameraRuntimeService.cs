using UnityEngine;

namespace Features.Camera.Domain
{
    /// <summary>
    /// Runtime-сервис камеры:
    /// - активная камера
    /// - FOV управление
    /// - Shake
    /// — НЕ отвечает за yaw/pitch/FPS/TPS!!!
    /// </summary>
    public interface ICameraRuntimeService
    {
        UnityEngine.Camera CurrentCamera { get; }

        void SetCamera(UnityEngine.Camera cam);
        void ClearCamera();

        void SetFOV(float fov);
        void AddFOV(float delta);

        void RequestShake(float intensity, float duration);

        float ConsumeShakeIntensity();
        float ConsumeShakeDuration();
    }
}
