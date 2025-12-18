using UnityEngine;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform fpsPoint;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform cameraTransform;

        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float tpsSmoothSpeed = 7f;

        [Header("TPS Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;

        [Header("TPS Body Turn Limit")]
        [SerializeField] private float bodyTurnLimit = 40f;

        // ⚠️ НЕ кешируем сервис — берём лениво
        private ICameraControlService Service
            => CameraServiceProvider.Control;

        private float currentTpsDistance = 3f;
        private bool isReady;

        private void Start()
        {
            currentTpsDistance = 3f;
            isReady = true;
        }

        // ======================================================
        // INPUT
        // ======================================================

        public void SetLookInput(Vector2 input)
        {
            if (!isReady)
                return;

            TryResolveCamera();

            if (cameraTransform == null ||
                cameraPivot == null ||
                playerBody == null)
                return;

            Service?.SetLookInput(input, mouseSensitivity, Time.deltaTime);
        }

        public void SwitchView()
        {
            if (!isReady)
                return;

            Service?.SwitchView();
        }

        // ======================================================
        // UPDATE
        // ======================================================

        private void LateUpdate()
        {
            if (!isReady)
                return;

            TryResolveCamera();

            if (cameraTransform == null ||
                cameraPivot == null ||
                playerBody == null)
                return;

            var svc = Service;
            if (svc == null)
                return;

            svc.UpdateTransition(Time.deltaTime);

            float blend = svc.State.Blend;

            if (blend < 0.5f)
                UpdateFPS(svc);
            else
                UpdateTPS(svc);
        }

        // ======================================================
        // FPS
        // ======================================================

        private void UpdateFPS(ICameraControlService svc)
        {
            if (cameraTransform == null || fpsPoint == null)
                return;

            svc.UpdateRotationFPS(playerBody);

            Vector3 camEuler = new Vector3(
                svc.State.Pitch,
                svc.State.Yaw,
                0f
            );

            cameraTransform.position = fpsPoint.position;
            cameraTransform.rotation = Quaternion.Euler(camEuler);

            if (headTransform != null)
                headTransform.localRotation =
                    Quaternion.Euler(svc.State.Pitch, 0f, 0f);
        }

        // ======================================================
        // TPS
        // ======================================================

        private void UpdateTPS(ICameraControlService svc)
        {
            if (cameraTransform == null || cameraPivot == null)
                return;

            svc.UpdateRotationTPS(cameraPivot, playerBody, bodyTurnLimit);

            Vector3 desired =
                cameraPivot.position - cameraPivot.forward * currentTpsDistance;

            float targetDistance = svc.ComputeTpsDistance(
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

            cameraTransform.rotation = cameraPivot.rotation;

            if (headTransform != null)
                headTransform.rotation = cameraPivot.rotation;
        }

        // ======================================================
        // CAMERA RESOLVE
        // ======================================================

        private void TryResolveCamera()
        {
            if (cameraTransform != null)
                return;

            if (CameraRegistry.Instance != null &&
                CameraRegistry.Instance.CurrentCamera != null)
            {
                cameraTransform =
                    CameraRegistry.Instance.CurrentCamera.transform;
            }
        }
    }
}
