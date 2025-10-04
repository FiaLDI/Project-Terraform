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
            //TPS-view
            cameraPivot.Rotate(Vector3.up * mouseX, Space.World);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            Vector3 pivotEuler = cameraPivot.localEulerAngles;
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, pivotEuler.y, 0f);
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

