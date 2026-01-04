using UnityEngine;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Локальный контроллер камеры игрока.
    /// Управляет ГЛОБАЛЬНОЙ Unity Camera через CameraRegistry.
    /// НЕ шлёт RPC, НЕ двигает игрока напрямую.
    /// </summary>
    public sealed class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform fpsPoint;
        [SerializeField] private Transform headTransform;

        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float tpsSmoothSpeed = 7f;

        [Header("TPS Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;

        // ===== Runtime =====
        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;

        // 🔑 Сервис логики камеры
        private ICameraControlService control;

        private float currentTpsDistance = 3f;
        private bool isLocal;

        // 🔑 ТОЛЬКО для записи yaw в input state
        private PlayerMovementNetAdapter movementNet;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            enabled = false;

            control = new CameraControlService();
            movementNet = GetComponent<PlayerMovementNetAdapter>();
        }

        private void OnEnable()
        {
            ResolveCamera();
        }

        private void OnDisable()
        {
            unityCamera = null;
            cameraTransform = null;
        }

        // ======================================================
        // INPUT (вызывается InputHandler'ом)
        // ======================================================

        public void SetLookInput(Vector2 input)
        {
            if (!isLocal || cameraTransform == null)
                return;

            control.SetLookInput(input, mouseSensitivity, Time.deltaTime);

            float yaw = control.State.Yaw;

            Debug.Log(
                $"[LOOK][CLIENT] yaw={yaw:F1} pitch={control.State.Pitch:F1}",
                this
            );

            if (movementNet != null)
                movementNet.SetLookYaw(yaw);
        }

        public void SwitchView()
        {
            if (!isLocal)
                return;

            control.SwitchView();
        }

        // ======================================================
        // UPDATE
        // ======================================================

        private void LateUpdate()
        {
            if (!isLocal)
                return;

            if (cameraTransform == null && !ResolveCamera())
                return;

            control.UpdateTransition(Time.deltaTime);

            var state = control.State;

            if (state.Blend < 0.5f)
                UpdateFPS(state);
            else
                UpdateTPS(state);
        }

        // ======================================================
        // FPS
        // ======================================================

        private void UpdateFPS(PlayerCameraState state)
        {
            if (fpsPoint == null)
                return;

            cameraTransform.position = fpsPoint.position;
            cameraTransform.rotation =
                Quaternion.Euler(state.Pitch, state.Yaw, 0f);

            if (headTransform != null)
            {
                headTransform.localRotation =
                    Quaternion.Euler(state.Pitch, 0f, 0f);
            }
        }

        // ======================================================
        // TPS
        // ======================================================

        private void UpdateTPS(PlayerCameraState state)
        {
            if (cameraPivot == null)
                return;

            cameraPivot.localRotation =
                Quaternion.Euler(state.Pitch, 0f, 0f);

            Vector3 desired =
                cameraPivot.position - cameraPivot.forward * currentTpsDistance;

            float targetDistance = control.ComputeTpsDistance(
                cameraPivot.position,
                desired,
                collisionMask,
                collisionRadius,
                minCameraDistance
            );

            currentTpsDistance = Mathf.Lerp(
                currentTpsDistance,
                targetDistance,
                Time.deltaTime * tpsSmoothSpeed
            );

            cameraTransform.position =
                cameraPivot.position - cameraPivot.forward * currentTpsDistance;

            cameraTransform.rotation =
                Quaternion.Euler(state.Pitch, state.Yaw, 0f);

            if (headTransform != null)
                headTransform.rotation = cameraTransform.rotation;
        }

        // ======================================================
        // CAMERA RESOLVE
        // ======================================================

        private bool ResolveCamera()
        {
            if (unityCamera != null)
                return true;

            if (CameraRegistry.Instance == null)
                return false;

            unityCamera = CameraRegistry.Instance.CurrentCamera;
            if (unityCamera == null)
                return false;

            cameraTransform = unityCamera.transform;
            return true;
        }

        // ======================================================
        // LOCAL CONTROL
        // ======================================================

        public void SetLocal(bool value)
        {
            if (isLocal == value)
                return;

            isLocal = value;
            enabled = value;

            Debug.Log(
                $"[PlayerCameraController] {name} SetLocal={value}"
            );
        }
    }
}
