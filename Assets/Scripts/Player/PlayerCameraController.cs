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
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField][Range(-90f, 0f)] private float minPitch = -60f;
    [SerializeField][Range(0f, 90f)] private float maxPitch = 80f;
    [SerializeField] private float switchSpeed = 7f;
    [Header("Collision (TPS)")]
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraCollisionRadius = 0.3f;
    [SerializeField] private float minCameraDistance = 0.5f;

    [Header("Body rotation (TPS)")]
    [SerializeField] private float headYawLimit = 90f;   
    [SerializeField] private float bodyTurnSpeed = 5f;     

    private bool isFirstPerson = true;
    private bool justSwitchedView = false;
    private float tpsCamDistance = 3f;
    private float cameraPitch = 0f;
    private Vector2 lookInput = Vector2.zero;

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
        justSwitchedView = true;
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
            //FPS-view
            playerBody.Rotate(Vector3.up * mouseX);

            cameraPitch += mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            if (headTransform != null)
            {
                headTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }

        }
        else
        {
            // === TPS view ===

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
                float limitedAngle = Mathf.Clamp(angle, -40f, 40f);
                headTransform.localRotation = Quaternion.Euler(cameraPitch, limitedAngle, 0f);
            }
            if (Mathf.Abs(angle) > 40f)
            {
                float turnDir = Mathf.Sign(angle);
                float turnAmount = turnDir * 120f * Time.deltaTime;

                float oldYaw = playerBody.eulerAngles.y;
                playerBody.Rotate(Vector3.up * turnAmount);
                float newYaw = playerBody.eulerAngles.y;

                float appliedDelta = Mathf.DeltaAngle(oldYaw, newYaw);
                headTransform.Rotate(Vector3.up, -appliedDelta, Space.World);

                bodyForward = playerBody.forward;
                bodyForward.y = 0;
                angle = Vector3.SignedAngle(bodyForward, cameraPivot.forward, Vector3.up);

                if (Mathf.Abs(angle) <= 90f)
                {
                    float limitedAngle = Mathf.Clamp(angle, -90f, 90f);
                    headTransform.localRotation = Quaternion.Euler(cameraPitch, limitedAngle, 0f);
                }
            }
        }
    }

    
    private void HandleCameraPosition()
    {
        Transform currentFpsPoint = fpsPoint;
        //Transform currentFpsPoint = playerMovement != null && playerMovement.IsCrouching ? crouchFpsPoint : fpsPoint;
        Transform currentTpsPoint = playerMovement != null && playerMovement.IsCrouching ? crouchTpsPoint : tpsPoint;

        if (isFirstPerson)
        {
            playerCamera.position = currentFpsPoint.position;
            playerCamera.rotation = currentFpsPoint.rotation;
        }
        else
        {
            Vector3 desiredDir = -cameraPivot.forward;
            float targetDistance = Vector3.Distance(cameraPivot.position, currentTpsPoint.position);

            if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, desiredDir, out RaycastHit hit, targetDistance, cameraCollisionMask))
            {
                targetDistance = Mathf.Max(hit.distance - 0.05f, minCameraDistance);
            }

            tpsCamDistance = Mathf.Lerp(tpsCamDistance, targetDistance, Time.deltaTime * switchSpeed);
            playerCamera.position = cameraPivot.position + desiredDir * tpsCamDistance;
            playerCamera.rotation = cameraPivot.rotation;
        }
    }

}

