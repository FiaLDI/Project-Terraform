using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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

        [Header("FPS View")]
        [SerializeField] private Transform viewModelRoot;
        [SerializeField] private GameObject fpsArmsPrefab;

        private Transform weaponSocket;
        public Transform WeaponSocket => weaponSocket;
        public event Action<UnityEngine.Camera> OnCameraChanged;
        public event Action<bool> OnFPSModeChanged;

        private GameObject fpsInstance;
        public bool IsFPSActive => fpsInstance != null && fpsInstance.activeSelf;

        private void Awake()
        {
            Debug.Log($"[camera-fix] CameraRegistry Awake | sceneCamera NULL? {sceneCamera == null}");
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[CameraRegistry] Duplicate detected on {name}, destroying"
                );
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            if (sceneCamera == null)
            {
                Debug.LogError(
                    "[CameraRegistry] Scene camera is NOT assigned!"
                );
                return;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;

            RegisterCamera(sceneCamera);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetFPS();
        }

        private void ResetFPS()
        {
            if (fpsInstance != null)
                Destroy(fpsInstance);

            fpsInstance = null;
            weaponSocket = null;
            OnFPSModeChanged?.Invoke(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnregisterCurrent();
                Instance = null;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ======================================================
        // REGISTRATION
        // ======================================================

        public void RegisterCamera(UnityEngine.Camera cam)
        {
            Debug.Log($"[camera-fix] RegisterCamera {cam.name}");
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

        public void InitializeFPS()
        {
            if (CurrentCamera == null)
                return;

            if (viewModelRoot == null)
            {
                Debug.LogError("[CameraRegistry] ViewModelRoot not assigned");
                return;
            }

            if (fpsArmsPrefab == null)
                return;

            if (fpsInstance != null)
                return;

            fpsInstance = Instantiate(fpsArmsPrefab, viewModelRoot);
            fpsInstance.transform.localPosition = Vector3.zero;
            fpsInstance.transform.localRotation = Quaternion.identity;

            weaponSocket = fpsInstance.transform.Find("WeaponSocket");

            OnFPSModeChanged?.Invoke(true);
        }
    
        public void SetFPSVisible(bool visible)
        {
            if (fpsInstance == null)
                return;

            fpsInstance.SetActive(visible);
            OnFPSModeChanged?.Invoke(visible);
        }
    }
}
