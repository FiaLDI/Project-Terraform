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

        private ICameraControlService service;

        private float currentTpsDistance = 3f;
        private bool isReady = false;

        private void Awake()
        {
            service = CameraServiceProvider.Control;
        }

        private void Start()
        {
            currentTpsDistance = 3f;
            isReady = true;
        }

        public void SetLookInput(Vector2 input)
        {
            if (!isReady) return;

            TryResolveCamera();

            if (cameraTransform == null ||
                cameraPivot == null ||
                playerBody == null)
                return;

            service?.SetLookInput(input, mouseSensitivity, Time.deltaTime);
        }

        private void TryResolveCamera()
        {
            if (cameraTransform != null) return;

            if (CameraRegistry.Instance != null &&
                CameraRegistry.Instance.CurrentCamera != null)
            {
                cameraTransform = CameraRegistry.Instance.CurrentCamera.transform;
            }
        }

        public void SwitchView()
        {
            if (!isReady) return;
            service?.SwitchView();
        }

        private void LateUpdate()
        {
            if (!isReady) return;

            TryResolveCamera();

            if (cameraTransform == null || cameraPivot == null || playerBody == null)
                return;

            if (service == null) return;

            service.UpdateTransition(Time.deltaTime);

            float blend = service.State.Blend;

            if (blend < 0.5f)
                UpdateFPS();
            else
                UpdateTPS();
        }

        private void UpdateFPS()
        {
            if (cameraTransform == null || fpsPoint == null) return;

            service.UpdateRotationFPS(playerBody);

            Vector3 camEuler = new Vector3(service.State.Pitch, service.State.Yaw, 0f);

            cameraTransform.position = fpsPoint.position;
            cameraTransform.rotation = Quaternion.Euler(camEuler);

            if (headTransform != null)
                headTransform.localRotation = Quaternion.Euler(service.State.Pitch, 0, 0);
        }

        private void UpdateTPS()
        {
            if (cameraTransform == null || cameraPivot == null) return;

            service.UpdateRotationTPS(cameraPivot, playerBody, bodyTurnLimit);

            Vector3 desired = cameraPivot.position - cameraPivot.forward * currentTpsDistance;

            float targetDistance = service.ComputeTpsDistance(
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
    }
}
