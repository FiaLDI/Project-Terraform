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
        [SerializeField] private Transform visualRoot;

        [Header("TPS")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0f, 0f);
        [SerializeField] private float maxTpsDistance = 5f;

        [Header("FOV")]
        [SerializeField] private float baseFov = 75f;
        [SerializeField] private float sprintFov = 90f;
        [SerializeField] private float fovSpeed = 8f;
        [SerializeField] private LayerMask fpsMask;
        [SerializeField] private LayerMask tpsMask;

        [Header("ADS")]
        [SerializeField] private float adsFov = 65f;
        [SerializeField] private float adsDistance = 2f;

        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;
        private ICameraControlService control;
        private ICameraControlService Control => CameraServiceProvider.Control;

        private float currentTpsDistance = 5f;

        private bool isLocal;
        private bool isAiming;

        private float smoothYaw;
        private float smoothPitch;
        private float yawVelocity;
        private float pitchVelocity;

        private const float BASE_MULTIPLIER = 100f;

        private void Awake()
        {
            enabled = false;
            control = CameraServiceProvider.Control;
        }

        public void SetLocal(bool value)
        {
            isLocal = value;
            enabled = value;

            if (value)
                ResolveCamera();
        }

        public void SetAiming(bool value)
        {
            isAiming = value;
        }

        public void SetHead(Transform head)
        {
            headTransform = head;
        }

        private void LateUpdate()
        {
            if (!isLocal || Control == null)
                return;

            if (cameraTransform == null && !ResolveCamera())
                return;

            control.UpdateTransition(Time.deltaTime);

            var state = control.State;

            bool isFPS = state.Blend < 0.5f;

            if (unityCamera != null)
                unityCamera.cullingMask = isFPS ? fpsMask : tpsMask;

            float smoothTime = 0.035f;

            smoothYaw = state.Yaw;

            smoothPitch = Mathf.SmoothDamp(
                smoothPitch,
                state.Pitch,
                ref pitchVelocity,
                smoothTime
            );

            if (isFPS)
                UpdateFPS();
            else
                UpdateTPS();

            UpdateFOV();
        }

        // ================= FPS =================

        private void UpdateFPS()
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                fpsPoint.position,
                1f - Mathf.Exp(-20f * Time.deltaTime)
            );

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                Quaternion.Euler(smoothPitch, smoothYaw, 0f),
                1f - Mathf.Exp(-20f * Time.deltaTime)
            );
        }

        // ================= TPS =================

        private void UpdateTPS()
        {
            cameraPivot.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);

            Vector3 pivotPos = cameraPivot.position;

            // 🔥 движение назад ТОЛЬКО по yaw (фикс "камера у пола")
            Vector3 yawForward = Quaternion.Euler(0f, smoothYaw, 0f) * Vector3.forward;

            Vector3 desired = pivotPos - yawForward * maxTpsDistance;

            float targetDistance = control.ComputeTpsDistance(
                pivotPos,
                desired,
                collisionMask,
                collisionRadius,
                minCameraDistance
            );

            if (isAiming)
                targetDistance = Mathf.Min(targetDistance, adsDistance);

            currentTpsDistance = Mathf.Lerp(
                currentTpsDistance,
                targetDistance,
                1f - Mathf.Exp(-10f * Time.deltaTime)
            );

            Vector3 shoulderWorld =
                pivotPos + cameraPivot.rotation * shoulderOffset;

            Vector3 targetPos =
                shoulderWorld - yawForward * currentTpsDistance;

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPos,
                1f - Mathf.Exp(-15f * Time.deltaTime)
            );

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                Quaternion.Euler(smoothPitch, smoothYaw, 0f),
                1f - Mathf.Exp(-15f * Time.deltaTime)
            );
        }

        // ================= FOV =================

        private void UpdateFOV()
        {
            if (unityCamera == null)
                return;

            var movement = GetComponent<DeterministicMovement>();

            bool isSprinting =
                movement != null &&
                movement.CurrentMaxSpeed > 6f &&
                movement.Velocity.magnitude > 0.1f;

            float targetFov = isAiming
                ? adsFov
                : isSprinting
                    ? sprintFov
                    : baseFov;

            unityCamera.fieldOfView = Mathf.Lerp(
                unityCamera.fieldOfView,
                targetFov,
                1f - Mathf.Exp(-fovSpeed * Time.deltaTime)
            );
        }

        // ================= CAMERA =================

        private bool ResolveCamera()
        {
            if (CameraRegistry.Instance == null)
                return false;

            var cam = CameraRegistry.Instance.CurrentCamera;
            if (cam == null)
                return false;

            unityCamera = cam;
            cameraTransform = cam.transform;

            return true;
        }

        // ================= INPUT =================

        public void SetLookInput(Vector2 input)
        {
            if (!isLocal || control == null)
                return;

            float sens = SettingsStorage.Sensitivity * BASE_MULTIPLIER;

            control.SetLookInput(input, sens, Time.deltaTime);

            var handler = FindObjectOfType<MovementInputHandler>();
            if (handler != null)
            {
                handler.SetYaw(control.State.Yaw);
                handler.SetPitch(control.State.Pitch);
            }
        }

        public void SwitchView()
        {
            if (!isLocal || control == null)
                return;

            control.SwitchView();
        }

        // ================= SETTINGS =================

        public void RefreshSensitivity()
        {
            // ничего сложного — просто оставляем совместимость
            // (можно позже расширить)
        }

        // ================= SYSTEM =================

        public void ForceReattachCamera()
        {
            ResolveCamera();
        }
    }
}
