using UnityEngine;
using Features.Camera.Application;
using Features.Camera.Domain;
using Features.Camera.UnityIntegration;
using System.Collections;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform fpsPoint;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform headYawBone;
        [SerializeField] private float headYawLimit = 40f;
        [SerializeField] private float bodyTurnSpeed = 360f;

        [SerializeField] private float tpsSmoothSpeed = 7f;

        [Header("TPS Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;
        private float currentSensitivity;

        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0f, 0f);

        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;
        private ICameraControlService control;

        private float currentTpsDistance = 5f;
        private bool isLocal;

        private ICameraControlService Control => CameraServiceProvider.Control;

        private const float BASE_MULTIPLIER = 100f;
        private float bodyYaw;
private float headYawOffset;

        private void Start()
        {
            currentSensitivity = SettingsStorage.Sensitivity * BASE_MULTIPLIER;
        }

        public void RefreshSensitivity()
        {
            currentSensitivity = SettingsStorage.Sensitivity * BASE_MULTIPLIER;
        }

        private void Awake()
        {
            Debug.Log($"[camera-fix] {name} Awake | Control NULL? {CameraServiceProvider.Control == null}");

            enabled = false;
            control = CameraServiceProvider.Control;
        }

        private void OnDestroy()
        {
            Debug.Log($"[camera-fix] {name} DESTROYED | isLocal={isLocal}");
        }

        public void SetLookInput(Vector2 input)
        {
            if (!isLocal || control == null)
                return;

            float sens = SettingsStorage.Sensitivity * BASE_MULTIPLIER;
            control.SetLookInput(input, sens, Time.deltaTime);

            float yaw = control.State.Yaw;
            float pitch = control.State.Pitch;

            var handler = FindObjectOfType<MovementInputHandler>();
            handler?.SetYaw(yaw);
            handler?.SetPitch(pitch);
        }

        public void SwitchView()
        {
            if (!isLocal || control == null)
                return;

            control.SwitchView();
        }

        private void LateUpdate()
        {
            if (!isLocal)
            {
                Debug.Log($"[camera-fix] {name} LateUpdate SKIP not local");
                return;
            }

            if (Control == null)
            {
                Debug.Log($"[camera-fix] {name} LateUpdate SKIP Control NULL");
                return;
            }

            if (cameraTransform == null)
            {
                Debug.Log($"[camera-fix] {name} LateUpdate cameraTransform NULL");
                return;
            }

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
            cameraTransform.position = fpsPoint.position;
            cameraTransform.rotation =
                Quaternion.Euler(state.Pitch, state.Yaw, 0f);

            if (headTransform != null)
                headTransform.localRotation =
                    Quaternion.Euler(state.Pitch, 0f, 0f);
        }

        private void UpdateTPS(PlayerCameraState state)
        {
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

            currentTpsDistance = targetDistance;

            Vector3 shoulderWorld =
                cameraPivot.TransformPoint(shoulderOffset);

            Vector3 targetPos =
                shoulderWorld - cameraPivot.forward * currentTpsDistance;

            cameraTransform.position = targetPos;
            cameraTransform.rotation =
            Quaternion.Euler(state.Pitch, state.Yaw, 0f);
        }

        private bool ResolveCamera()
        {
            if (CameraRegistry.Instance == null)
                return false;

            var cam = CameraRegistry.Instance.CurrentCamera;
            if (cam == null)
                return false;

            unityCamera = cam;
            cameraTransform = cam.transform;

            Debug.Log($"[Camera] Attached to {name}");

            return true;
        }

        public void SetLocal(bool value)
        {
            isLocal = value;
            enabled = value;

            if (value)
                ResolveCamera();
        }

        public void ForceReattachCamera()
        {
            ResolveCamera();
        }
    }
}
