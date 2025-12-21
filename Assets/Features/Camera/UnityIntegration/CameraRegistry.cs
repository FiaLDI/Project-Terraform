using UnityEngine;
using System;

namespace Features.Camera.UnityIntegration
{
    /// <summary>
    /// Глобальный реестр ЕДИНСТВЕННОЙ Unity Camera.
    /// Камера существует в сцене в одном экземпляре.
    /// Player никогда не создаёт камеру.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class CameraRegistry : MonoBehaviour
    {
        public static CameraRegistry Instance { get; private set; }

        [SerializeField]
        private UnityEngine.Camera sceneCamera;

        public UnityEngine.Camera CurrentCamera { get; private set; }
        public event Action<UnityEngine.Camera> OnCameraChanged;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[CameraRegistry] Duplicate detected on {name}, destroying"
                );
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (sceneCamera == null)
            {
                Debug.LogError(
                    "[CameraRegistry] Scene camera is NOT assigned!"
                );
                return;
            }

            RegisterCamera(sceneCamera);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnregisterCurrent();
                Instance = null;
            }
        }

        // ======================================================
        // REGISTRATION
        // ======================================================

        public void RegisterCamera(UnityEngine.Camera cam)
        {
            if (cam == null)
            {
                Debug.LogError("[CameraRegistry] Tried to register NULL camera");
                return;
            }

            if (CurrentCamera == cam)
                return;

            if (CurrentCamera != null)
            {
                Debug.LogWarning(
                    $"[CameraRegistry] Camera already registered ({CurrentCamera.name}), " +
                    $"replacing with {cam.name}"
                );
            }

            CurrentCamera = cam;

            CameraServiceProvider.Runtime?.SetCamera(cam);
            OnCameraChanged?.Invoke(cam);

            Debug.Log($"[CameraRegistry] Registered camera: {cam.name}");
        }

        private void UnregisterCurrent()
        {
            if (CurrentCamera == null)
                return;

            Debug.Log($"[CameraRegistry] Unregistered camera: {CurrentCamera.name}");

            CameraServiceProvider.Runtime?.ClearCamera();
            CurrentCamera = null;
            OnCameraChanged?.Invoke(null);
        }
    }
}
