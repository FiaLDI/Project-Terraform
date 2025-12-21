using UnityEngine;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Локальный контроллер камеры игрока.
    /// Управляет ГЛОБАЛЬНОЙ Unity Camera через CameraRegistry.
    /// </summary>
    public sealed class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform fpsPoint;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform headTransform;

        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float tpsSmoothSpeed = 7f;

        [Header("TPS Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;

        [Header("TPS Body Turn Limit")]
        [SerializeField] private float bodyTurnLimit = 40f;

        // ===== Runtime =====
        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;

        // 🔑 УНИКАЛЬНЫЙ control-сервис на игрока
        private ICameraControlService control;

        private float currentTpsDistance = 3f;
        private bool isLocal;

        private PlayerMovementNetAdapter movementNet;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            // По умолчанию ВЫКЛЮЧЕН
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
        // INPUT (из PlayerController)
        // ======================================================

        public void SetLookInput(Vector2 input)
        {
            if (!isLocal || cameraTransform == null)
                return;

            control.SetLookInput(input, mouseSensitivity, Time.deltaTime);

            // Передаём yaw серверу для поворота тела
            if (movementNet != null)
            {
                movementNet.SetBodyRotation(control.State.Yaw);
            }
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
            cameraTransform.rotation = Quaternion.Euler(state.Pitch, state.Yaw, 0f);

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
        // LOCAL CONTROL (из PlayerCameraNetAdapter)
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
