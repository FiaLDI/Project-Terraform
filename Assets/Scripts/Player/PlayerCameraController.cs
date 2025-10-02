using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
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
        playerBody.Rotate(Vector3.up * mouseX);

        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (isFirstPerson)
        {
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
        else
        {
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleCameraPosition()
    {
        Transform currentFpsPoint = playerMovement != null && playerMovement.IsCrouching ? crouchFpsPoint : fpsPoint;
        Transform currentTpsPoint = playerMovement != null && playerMovement.IsCrouching ? crouchTpsPoint : tpsPoint;

        if (isFirstPerson)
        {
            if (justSwitchedView)
            {
                playerCamera.position = currentFpsPoint.position;
                playerCamera.rotation = currentFpsPoint.rotation;
                justSwitchedView = false;
            }
            else
            {
                playerCamera.position = currentFpsPoint.position;
                playerCamera.rotation = currentFpsPoint.rotation;
            }
        }
        else
        {
            Vector3 pivotPos = cameraPivot.position;
            Vector3 desiredDir = (currentTpsPoint.position - pivotPos).normalized;
            float desiredDist = Vector3.Distance(pivotPos, currentTpsPoint.position);
            float targetDistance = desiredDist;

            if (Physics.SphereCast(pivotPos, cameraCollisionRadius, desiredDir, out RaycastHit hit, desiredDist, cameraCollisionMask))
            {
                targetDistance = Mathf.Max(hit.distance - 0.05f, minCameraDistance);
            }
            else
            {
                targetDistance = desiredDist;
            }

            tpsCamDistance = Mathf.Lerp(tpsCamDistance, targetDistance, Time.deltaTime * switchSpeed);
            Vector3 targetPos = pivotPos + desiredDir * tpsCamDistance;
            playerCamera.position = targetPos;
            playerCamera.LookAt(cameraPivot);
        }
    }
}
