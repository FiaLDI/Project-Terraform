using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform fpsPoint;
    [SerializeField] private Transform tpsPoint;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform crouchFpsPoint;
    [SerializeField] private Transform crouchTpsPoint;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 80f;
    [SerializeField, Range(-90f, 0f)] private float minPitch = -60f;
    [SerializeField, Range(0f, 90f)] private float maxPitch = 80f;
    [SerializeField] private float switchSpeed = 7f;

    [Header("Collision (TPS)")]
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraCollisionRadius = 0.3f;
    [SerializeField] private float minCameraDistance = 0.5f;

    [Header("Body rotation (TPS)")]
    [SerializeField] private float headYawLimit = 90f;
    [SerializeField] private float bodyTurnSpeed = 5f;
    [SerializeField, Range(0f, 180f)] private float bodyTurnAngleLimit = 40f;

    [Header("Head Bob (FPS)")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float walkBobAmplitude = 0.03f;
    [SerializeField] private float walkBobFrequency = 7f;
    [SerializeField] private float runBobAmplitude = 0.06f;
    [SerializeField] private float runBobFrequency = 11f;
    [SerializeField] private float crouchBobMultiplier = 0.5f;
    [SerializeField] private float bobSmoothing = 10f;

    private bool isFirstPerson = true;
    private float tpsCamDistance = 3f;
    private float cameraPitch = 0f;
    private Vector2 lookInput = Vector2.zero;

    private float headBobTimer = 0f;
    private Vector3 currentBobOffset = Vector3.zero;

    private void Start()
    {
        Vector3 pivotPos = cameraPivot.position;
        tpsCamDistance = Vector3.Distance(pivotPos, tpsPoint.position);
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    public void SwitchView()
    {
        isFirstPerson = !isFirstPerson;
    }

    private void LateUpdate()
    {
        HandleLook();
        HandleCameraPosition();
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        if (isFirstPerson)
        {
            // FPS: тело вращаем по Y
            playerBody.Rotate(Vector3.up * mouseX);

            // pitch камеры (через голову)
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            if (headTransform != null)
            {
                Quaternion targetHeadRot = Quaternion.Euler(cameraPitch, 0f, 0f);
                headTransform.localRotation = Quaternion.Slerp(
                    headTransform.localRotation,
                    targetHeadRot,
                    Time.deltaTime * 12f
                );
            }
        }
        else
        {
            // ===== TPS VIEW (как было) =====

            cameraPivot.Rotate(Vector3.up * mouseX, Space.World);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            Vector3 pivotEuler = cameraPivot.localEulerAngles;
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, pivotEuler.y, 0f);

            Vector3 bodyForward = playerBody.forward;
            Vector3 camForward = cameraPivot.forward;
            bodyForward.y = 0;
            camForward.y = 0;

            float angle = Vector3.SignedAngle(bodyForward, camForward, Vector3.up);

            if (headTransform != null)
            {
                float limitedAngle = Mathf.Clamp(angle, -headYawLimit, headYawLimit);
                headTransform.localRotation = Quaternion.Euler(cameraPitch, limitedAngle, 0f);
            }

            if (Mathf.Abs(angle) > bodyTurnAngleLimit)
            {
                float turnDir = Mathf.Sign(angle);
                float turnAmount = turnDir * bodyTurnSpeed * 120f * Time.deltaTime;

                float oldYaw = playerBody.eulerAngles.y;
                playerBody.Rotate(Vector3.up * turnAmount);
                float newYaw = playerBody.eulerAngles.y;

                float appliedDelta = Mathf.DeltaAngle(oldYaw, newYaw);
                cameraPivot.Rotate(Vector3.up, -appliedDelta, Space.World);

                bodyForward = playerBody.forward;
                bodyForward.y = 0;
                angle = Vector3.SignedAngle(bodyForward, cameraPivot.forward, Vector3.up);

                if (Mathf.Abs(angle) <= bodyTurnAngleLimit)
                {
                    float limitedAngle = Mathf.Clamp(angle, -bodyTurnAngleLimit, bodyTurnAngleLimit);
                    cameraPivot.localRotation = Quaternion.Euler(cameraPitch, limitedAngle, 0f);
                }
            }
        }
    }

    private void HandleCameraPosition()
    {
        Transform currentFpsPoint =
            playerMovement != null && playerMovement.IsCrouching ? crouchFpsPoint : fpsPoint;
        Transform currentTpsPoint =
            playerMovement != null && playerMovement.IsCrouching ? crouchTpsPoint : tpsPoint;

        if (isFirstPerson)
        {
            ApplyFirstPersonCamera(currentFpsPoint);
        }
        else
        {
            ApplyThirdPersonCamera(currentTpsPoint);
        }
    }

    private void ApplyFirstPersonCamera(Transform currentFpsPoint)
    {
        Vector3 basePos = currentFpsPoint.position;
        Quaternion baseRot = currentFpsPoint.rotation;

        // === AAA Head-Bob ===
        Vector3 targetBobOffset = Vector3.zero;

        if (enableHeadBob && playerMovement != null && playerMovement.IsGrounded && playerMovement.PlanarSpeed > 0.1f)
        {
            bool sprinting = playerMovement.IsSprinting;

            float amp = sprinting ? runBobAmplitude : walkBobAmplitude;
            float freq = sprinting ? runBobFrequency : walkBobFrequency;

            if (playerMovement.IsCrouching)
                amp *= crouchBobMultiplier;

            headBobTimer += Time.deltaTime * freq;

            // классический синусовый боббинг
            float bobY = Mathf.Sin(headBobTimer * 2f) * amp;          // вверх-вниз
            float bobX = Mathf.Sin(headBobTimer) * amp * 0.5f;        // лёгкое в стороны

            Vector3 localOffset = new Vector3(bobX, bobY, 0f);
            // в мировые координаты
            targetBobOffset = currentFpsPoint.TransformDirection(localOffset);
        }
        else
        {
            headBobTimer = 0f;
        }

        // плавно догоняем целевой offset
        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * bobSmoothing);

        playerCamera.position = basePos + currentBobOffset;
        playerCamera.rotation = baseRot;
    }

    private void ApplyThirdPersonCamera(Transform currentTpsPoint)
    {
        Vector3 desiredDir = -cameraPivot.forward;
        float targetDistance = Vector3.Distance(cameraPivot.position, currentTpsPoint.position);

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, desiredDir,
            out RaycastHit hit, targetDistance, cameraCollisionMask))
        {
            targetDistance = Mathf.Max(hit.distance - 0.05f, minCameraDistance);
        }

        tpsCamDistance = Mathf.Lerp(tpsCamDistance, targetDistance, Time.deltaTime * switchSpeed);
        playerCamera.position = cameraPivot.position + desiredDir * tpsCamDistance;
        playerCamera.rotation = cameraPivot.rotation;
    }
}
