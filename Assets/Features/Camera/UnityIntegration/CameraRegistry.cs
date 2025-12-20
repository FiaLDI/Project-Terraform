using UnityEngine;
using System;

namespace Features.Camera.UnityIntegration
{
    public class CameraRegistry : MonoBehaviour
    {
        public static CameraRegistry Instance { get; private set; }

        public UnityEngine.Camera CurrentCamera { get; private set; }
        public event Action<UnityEngine.Camera> OnCameraChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterCamera(UnityEngine.Camera cam)
        {
            CurrentCamera = cam;

            // передаём в runtime-службу
            CameraServiceProvider.Runtime?.SetCamera(cam);

            OnCameraChanged?.Invoke(cam);
        }

        public void UnregisterCamera(UnityEngine.Camera cam)
        {
            if (cam == CurrentCamera)
            {
                if (CurrentCamera != null) {
                    CurrentCamera = null;

                    // удаляем ссылку из runtime-службы
                    CameraServiceProvider.Runtime?.ClearCamera();

                    OnCameraChanged?.Invoke(null);
                }
            }
        }
    }
}
