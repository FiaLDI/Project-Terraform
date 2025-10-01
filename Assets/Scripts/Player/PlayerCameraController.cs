using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Основные ссылки")]
    [SerializeField] private Transform playerCamera;   
    [SerializeField] private Transform playerBody;     
    [SerializeField] private Transform fpsPoint;       
    [SerializeField] private Transform tpsPoint;       
    [SerializeField] private Transform cameraPivot;    
    [SerializeField] private Transform crouchFpsPoint;
    [SerializeField] private Transform crouchTpsPoint;
    [SerializeField] private PlayerMovement playerMovement;
    [Header("Режимы камеры")]
    public bool isFirstPerson = true;
    [SerializeField] private float switchSpeed = 5f;   

    [Header("Чувствительность камеры")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField][Range(-90f, 0f)] private float minPitch = -60f;
    [SerializeField][Range(0f, 90f)] private float maxPitch = 80f;

    [Header("Столкновения камеры (TPS)")]
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraCollisionRadius = 0.3f;
    [SerializeField] private float minCameraDistance = 0.5f;

    private float cameraPitch = 0f;
    private Vector2 lookInput;

    private InputSystem_Actions inputActions;
    private bool justSwitchedView = false;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        
        inputActions.Player.SwitchView.performed += ctx =>
        {
            isFirstPerson = !isFirstPerson;
            justSwitchedView = true;
        };

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

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

        
        Transform currentFpsPoint = playerMovement.IsCrouching ? crouchFpsPoint : fpsPoint;
        Transform currentTpsPoint = playerMovement.IsCrouching ? crouchTpsPoint : tpsPoint;
        if (isFirstPerson)
        {
            
            if (justSwitchedView)
            {
                playerCamera.position = currentFpsPoint.position;
                playerCamera.rotation = currentTpsPoint.rotation;
                justSwitchedView = false;
            }
            else
            {
                playerCamera.position = Vector3.Lerp(playerCamera.position, currentFpsPoint.position, Time.deltaTime * switchSpeed);
                playerCamera.rotation = Quaternion.Lerp(playerCamera.rotation, currentFpsPoint.rotation, Time.deltaTime * switchSpeed);
            }
        }
        else
        {
            
            Vector3 pivotPos = cameraPivot.position;
            Vector3 desiredDir = (currentTpsPoint.position - pivotPos).normalized;
            float desiredDist = Vector3.Distance(pivotPos, currentTpsPoint.position);

            Vector3 targetPos = pivotPos + desiredDir * desiredDist;

            
            if (Physics.SphereCast(pivotPos, cameraCollisionRadius, desiredDir, out RaycastHit hit, desiredDist, cameraCollisionMask))
            {
                float adjustedDistance = Mathf.Max(hit.distance - 0.1f, minCameraDistance);
                targetPos = pivotPos + desiredDir * adjustedDistance;
            }

            playerCamera.position = targetPos;
            playerCamera.LookAt(cameraPivot);
        }
    }

}
