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
        [SerializeField] private GameObject fpsArmsOneHandPrefab;

        private Transform weaponSocket;
        public Transform WeaponSocket => weaponSocket;
        public event Action<UnityEngine.Camera> OnCameraChanged;
        public event Action<bool> OnFPSModeChanged;

        private GameObject fpsInstance;
        private GameObject currentFpsArmsPrefab;
        private Animator fpsArmsAnimator;
        private int weaponPose;
        private static readonly int ArmsOneHandPoseHash = Animator.StringToHash("arms_onehand_pose");
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
            currentFpsArmsPrefab = null;
            weaponSocket = null;
            fpsArmsAnimator = null;
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

            var prefab = SelectFpsArmsPrefab();
            if (prefab == null)
                return;

            if (fpsInstance != null && currentFpsArmsPrefab == prefab)
                return;

            bool keepVisible = fpsInstance == null || fpsInstance.activeSelf;

            if (fpsInstance != null)
                Destroy(fpsInstance);

            fpsInstance = Instantiate(prefab, viewModelRoot);
            fpsInstance.transform.localPosition = Vector3.zero;
            fpsInstance.transform.localRotation = Quaternion.identity;

            currentFpsArmsPrefab = prefab;
            weaponSocket = fpsInstance.transform.Find("WeaponSocket");
            fpsArmsAnimator = fpsInstance.GetComponentInChildren<Animator>(true);
            ApplyFpsArmsPose();
            fpsInstance.SetActive(keepVisible);

            OnFPSModeChanged?.Invoke(true);
        }
    
        public void SetFPSVisible(bool visible)
        {
            if (fpsInstance == null)
                return;

            fpsInstance.SetActive(visible);
            OnFPSModeChanged?.Invoke(visible);
        }

        private GameObject SelectFpsArmsPrefab()
        {
            if (weaponPose == 1 && fpsArmsOneHandPrefab != null)
                return fpsArmsOneHandPrefab;

            return fpsArmsPrefab;
        }

        public void SetWeaponPose(int pose)
        {
            int clamped = Mathf.Clamp(pose, 0, 2);
            if (weaponPose == clamped)
            {
                ApplyFpsArmsPose();
                return;
            }

            weaponPose = clamped;

            var desiredPrefab = SelectFpsArmsPrefab();
            if (fpsInstance != null && desiredPrefab != null && currentFpsArmsPrefab != desiredPrefab)
            {
                bool wasVisible = fpsInstance.activeSelf;
                InitializeFPS();
                if (fpsInstance != null)
                    fpsInstance.SetActive(wasVisible);
            }

            ApplyFpsArmsPose();
        }

        private void ApplyFpsArmsPose()
        {
            if (fpsArmsAnimator == null)
                return;

            bool oneHandPose = weaponPose == 1;

            for (int i = 0; i < fpsArmsAnimator.parameterCount; i++)
            {
                var parameter = fpsArmsAnimator.parameters[i];
                if (parameter.nameHash != ArmsOneHandPoseHash)
                    continue;

                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        fpsArmsAnimator.SetBool(ArmsOneHandPoseHash, oneHandPose);
                        break;

                    case AnimatorControllerParameterType.Int:
                        fpsArmsAnimator.SetInteger(ArmsOneHandPoseHash, oneHandPose ? 1 : 0);
                        break;

                    case AnimatorControllerParameterType.Float:
                        fpsArmsAnimator.SetFloat(ArmsOneHandPoseHash, oneHandPose ? 1f : 0f);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        if (oneHandPose)
                            fpsArmsAnimator.SetTrigger(ArmsOneHandPoseHash);
                        else
                            fpsArmsAnimator.ResetTrigger(ArmsOneHandPoseHash);
                        break;
                }

                return;
            }
        }
    }
}
