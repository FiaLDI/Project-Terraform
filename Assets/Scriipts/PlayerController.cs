using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Transform thirdPersonPivot;      // точка позади игрока (пустышка)
    public float thirdPersonDistance = 3f;  // базовое расстояние камеры
    public float minDistance = 0.5f;        // минимальная дистанция, если стена близко
    public float smooth = 10f;              // плавность смещения
    public LayerMask collisionMask;         // слои для коллизий

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private bool jumpPressed = false;
    private float xRotation = 0f;
    private bool isFirstPerson = true;
    private float currentDistance;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        SetCameraMode(true);
        currentDistance = thirdPersonDistance;
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = isRunning ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (isFirstPerson)
        {
            // Камера от 1-го лица
            firstPersonCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            // Камера от 3-го лица
            thirdPersonPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            HandleThirdPersonCollision();
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleThirdPersonCollision()
    {
        Vector3 pivotPos = thirdPersonPivot.position;
        Vector3 desiredCameraPos = pivotPos - thirdPersonPivot.forward * thirdPersonDistance;

        // Направление и расстояние
        Vector3 dir = desiredCameraPos - pivotPos;
        float distance = dir.magnitude;
        dir.Normalize();

        // Сразу ставим камеру на безопасное расстояние
        if (Physics.SphereCast(pivotPos, 0.3f, dir, out RaycastHit hit, thirdPersonDistance, collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, thirdPersonDistance);
        }
        else
        {
            currentDistance = thirdPersonDistance;
        }

        // Позиция камеры всегда безопасна
        thirdPersonCamera.transform.position = pivotPos - thirdPersonPivot.forward * currentDistance;
        thirdPersonCamera.transform.rotation = thirdPersonPivot.rotation;
    }



    private void SetCameraMode(bool firstPerson)
    {
        isFirstPerson = firstPerson;
        firstPersonCamera.enabled = firstPerson;
        thirdPersonCamera.enabled = !firstPerson;

        if (firstPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // ----- Input System -----
    public void OnMovement(InputAction.CallbackContext context) =>
        moveInput = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context) =>
        lookInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpPressed = true;
    }

    public void OnRun(InputAction.CallbackContext context) =>
        isRunning = context.ReadValueAsButton();

    public void OnSwitchView(InputAction.CallbackContext context)
    {
        if (context.performed)
            SetCameraMode(!isFirstPerson);
    }
}
