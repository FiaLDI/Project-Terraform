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

        [Header("TPS")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minCameraDistance = 0.5f;
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0f, 0f);

        [Header("FOV")]
        [SerializeField] private float baseFov = 75f;
        [SerializeField] private float sprintFov = 90f;
        [SerializeField] private float fovSpeed = 8f;
        [SerializeField] private LayerMask fpsMask;
        [SerializeField] private LayerMask tpsMask;
        [SerializeField] private float maxTpsDistance = 5f;

        private UnityEngine.Camera unityCamera;
        private Transform cameraTransform;
        private ICameraControlService control;

        private float currentTpsDistance = 5f;
        
        private bool isLocal;

        private float smoothYaw;
        private float smoothPitch;
        private float yawVelocity;
        private float pitchVelocity;

        private const float BASE_MULTIPLIER = 100f;
        [SerializeField] private float adsFov = 65f;
        [SerializeField] private float adsDistance = 2f;
        [SerializeField] private float adsSpeed = 10f;

        private bool isAiming;

        private ICameraControlService Control => CameraServiceProvider.Control;

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
            {
                unityCamera.cullingMask = isFPS ? fpsMask : tpsMask;
            }

            float smoothTime = 0.035f;

            smoothYaw = Mathf.SmoothDampAngle(
                smoothYaw,
                state.Yaw,
                ref yawVelocity,
                smoothTime
            );

            smoothPitch = Mathf.SmoothDamp(
                smoothPitch,
                state.Pitch,
                ref pitchVelocity,
                smoothTime
            );

            if (state.Blend < 0.5f)
                UpdateFPS();
            else
                UpdateTPS();

            UpdateFOV();

            CameraRegistry.Instance?.SetFPSVisible(state.Blend < 0.5f);
        }

        // ======================================================
        // FPS
        // ======================================================

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

            if (headTransform != null)
            {
                float headYawOffset = Mathf.DeltaAngle(
                    transform.eulerAngles.y,
                    smoothYaw
                );

                // ограничение (чтобы не ломалась шея)
                headYawOffset = Mathf.Clamp(headYawOffset, -80f, 80f);

                headTransform.localRotation = Quaternion.Euler(
                    smoothPitch,
                    headYawOffset,
                    0f
                );
            }
        }

        // ======================================================
        // TPS
        // ======================================================

        private void UpdateTPS()
        {
            cameraPivot.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);

            Vector3 desired =
                cameraPivot.position - cameraPivot.forward * maxTpsDistance;

            float targetDistance = control.ComputeTpsDistance(
                cameraPivot.position,
                desired,
                collisionMask,
                collisionRadius,
                minCameraDistance
            );

            if (isAiming)
            {
                targetDistance = Mathf.Min(targetDistance, adsDistance);
            }

            float speed = targetDistance > currentTpsDistance ? 4f : 15f;

            currentTpsDistance = Mathf.Lerp(
                currentTpsDistance,
                targetDistance,
                1f - Mathf.Exp(-speed * Time.deltaTime)
            );

            Vector3 shoulderWorld =
                cameraPivot.TransformPoint(shoulderOffset);

            Vector3 targetPos =
                shoulderWorld - cameraPivot.forward * currentTpsDistance;

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

            if (headTransform != null)
            {
                float rawOffset = Mathf.DeltaAngle(
                    transform.eulerAngles.y,
                    smoothYaw
                );

                float headYawOffset = rawOffset * 1.6f;

                headYawOffset = Mathf.Clamp(headYawOffset, -80f, 80f);

                float pitch = smoothPitch * 0.5f;

                headTransform.localRotation = Quaternion.Euler(
                    pitch,
                    headYawOffset,
                    0f
                );
            }
        }

        // ======================================================
        // DYNAMIC FOV
        // ======================================================

        private void UpdateFOV()
        {
            if (unityCamera == null)
                return;

            var movement = GetComponent<DeterministicMovement>();

            bool isSprinting =
                movement != null &&
                movement.CurrentMaxSpeed > 6f &&
                movement.Velocity.magnitude > 0.1f;

            float targetFov;

            if (isAiming)
                targetFov = adsFov;
            else if (isSprinting)
                targetFov = sprintFov;
            else
                targetFov = baseFov;

            unityCamera.fieldOfView = Mathf.Lerp(
                unityCamera.fieldOfView,
                targetFov,
                1f - Mathf.Exp(-fovSpeed * Time.deltaTime)
            );
        }

        // ======================================================
        // CAMERA RESOLVE
        // ======================================================

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
