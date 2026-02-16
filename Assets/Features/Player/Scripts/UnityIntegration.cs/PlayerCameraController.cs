using UnityEngine;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
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

        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;

        private ICameraControlService control;

        private float currentTpsDistance = 3f;
        private bool isLocal;

        private void Awake()
        {
            enabled = false;

            // 🔥 Используем глобальный сервис
            control = CameraServiceProvider.Control;
        }

        public void SetLookInput(Vector2 input)
        {
            if (!isLocal || control == null)
                return;

            control.SetLookInput(input, mouseSensitivity, Time.deltaTime);

            float yaw = control.State.Yaw;

            // 🔥 Передаём yaw в глобальный MovementInputHandler
            var handler = FindObjectOfType<MovementInputHandler>();
            handler?.SetYaw(yaw);
        }

        public void SwitchView()
        {
            if (!isLocal || control == null)
                return;

            control.SwitchView();
        }

        private void LateUpdate()
        {
            if (!isLocal || control == null)
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

        private void UpdateFPS(PlayerCameraState state)
        {
            if (fpsPoint == null)
                return;

            cameraTransform.position = fpsPoint.position;
            cameraTransform.rotation =
                Quaternion.Euler(state.Pitch, state.Yaw, 0f);

            if (headTransform != null)
                headTransform.localRotation =
                    Quaternion.Euler(state.Pitch, 0f, 0f);
        }

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

        public void SetLocal(bool value)
        {
            if (isLocal == value)
                return;

            isLocal = value;
            enabled = value;
        }
    }
}
