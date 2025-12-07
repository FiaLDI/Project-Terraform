using UnityEngine;
using Features.Camera.Domain;

namespace Features.Camera.Application
{
    /// <summary>
    /// Runtime-сервис камеры:
    /// хранит активную камеру, управляет FOV и Shake.
    /// НЕ отвечает за угол/позицию камеры!
    /// </summary>
    public class CameraRuntimeService : ICameraRuntimeService
    {
        public UnityEngine.Camera CurrentCamera { get; private set; }

        private float _shakeIntensity;
        private float _shakeDuration;

        public void SetCamera(UnityEngine.Camera cam)
        {
            CurrentCamera = cam;
        }

        public void ClearCamera()
        {
            CurrentCamera = null;
        }

        public void SetFOV(float fov)
        {
            if (CurrentCamera != null)
                CurrentCamera.fieldOfView = fov;
        }

        public void AddFOV(float delta)
        {
            if (CurrentCamera != null)
                CurrentCamera.fieldOfView += delta;
        }

        public void RequestShake(float intensity, float duration)
        {
            _shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
            _shakeDuration = Mathf.Max(_shakeDuration, duration);
        }

        public float ConsumeShakeIntensity()
        {
            float val = _shakeIntensity;
            _shakeIntensity = 0;
            return val;
        }

        public float ConsumeShakeDuration()
        {
            float val = _shakeDuration;
            _shakeDuration = 0;
            return val;
        }
    }
}
