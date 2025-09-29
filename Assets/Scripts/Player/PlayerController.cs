using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float sneakSpeed = 2f;      // 👈 Новая скорость для крадущейся ходьбы
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;    // 👈 Высота капсулы в приседе
    public float standHeight = 2f;     // 👈 Высота капсулы стоя
    public float crouchSpeed = 2.5f;   // 👈 Скорость при приседании
    public float crouchTransitionSpeed = 10f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Cameras")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Transform thirdPersonPivot;
    public float thirdPersonDistance = 3f;
    public float minDistance = 0.5f;
    public float smooth = 10f;
    public LayerMask collisionMask;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRunning;
    private bool isSneaking;
    private bool isCrouching;
    private bool jumpPressed = false;
    private float xRotation = 0f;
    private bool isFirstPerson = true;
    private float currentDistance;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // фиксируем высоту и центр коллайдера
        controller.height = standHeight;
        controller.center = new Vector3(0, controller.height / 2f, 0);

        // чтобы персонаж стоял на поверхности (исключаем проваливание)
        Vector3 pos = transform.position;
        pos.y = controller.height / 2f;
        transform.position = pos;

        SetCameraMode(true);
        currentDistance = thirdPersonDistance;

        // фиксируем начальное положение камеры под рост игрока
        Vector3 camPos = firstPersonCamera.transform.localPosition;
        camPos.y = standHeight - 0.2f;
        firstPersonCamera.transform.localPosition = camPos;

        Vector3 pivotPos = thirdPersonPivot.localPosition;
        pivotPos.y = standHeight - 0.2f;
        thirdPersonPivot.localPosition = pivotPos;
    }



    private void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleCrouch();
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        float speed = walkSpeed;

        if (isCrouching)
            speed = crouchSpeed;
        else if (isSneaking)
            speed = sneakSpeed;
        else if (isRunning)
            speed = runSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (jumpPressed && isGrounded && !isCrouching) // 👈 нельзя прыгнуть в приседе
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
            firstPersonCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            thirdPersonPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            HandleThirdPersonCollision();
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleThirdPersonCollision()
    {
        Vector3 pivotPos = thirdPersonPivot.position;
        Vector3 desiredCameraPos = pivotPos - thirdPersonPivot.forward * thirdPersonDistance;

        Vector3 dir = desiredCameraPos - pivotPos;
        float distance = dir.magnitude;
        dir.Normalize();

        if (Physics.SphereCast(pivotPos, 0.3f, dir, out RaycastHit hit, thirdPersonDistance, collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, thirdPersonDistance);
        }
        else
        {
            currentDistance = thirdPersonDistance;
        }

        thirdPersonCamera.transform.position = pivotPos - thirdPersonPivot.forward * currentDistance;
        thirdPersonCamera.transform.rotation = thirdPersonPivot.rotation;
    }

    private void HandleCrouch()
    {
        // целевая высота коллайдера
        float targetHeight = isCrouching ? crouchHeight : standHeight;

        // плавное изменение высоты капсулы
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        // --- Работа с камерами ---
        float targetCamY = isCrouching ? crouchHeight - 0.2f : standHeight - 0.2f;

        if (isFirstPerson)
        {
            // движение камеры от первого лица
            Vector3 camPos = firstPersonCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
            firstPersonCamera.transform.localPosition = camPos;
        }
        else
        {
            // движение pivot'а для камеры от третьего лица
            Vector3 pivotPos = thirdPersonPivot.localPosition;
            pivotPos.y = Mathf.Lerp(pivotPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
            thirdPersonPivot.localPosition = pivotPos;
        }
    }


    private void SetCameraMode(bool firstPerson)
    {
        isFirstPerson = firstPerson;
        firstPersonCamera.enabled = firstPerson;
        thirdPersonCamera.enabled = !firstPerson;

        if (firstPerson)
            Cursor.lockState = CursorLockMode.Locked;
    }

    // ==== INPUT ACTIONS ====

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

    public void OnSneak(InputAction.CallbackContext context) =>
        isSneaking = context.ReadValueAsButton();   // 👈 Sneak (X)

    public void OnCrouch(InputAction.CallbackContext context) =>
        isCrouching = context.ReadValueAsButton();  // 👈 Crouch (CTRL)

    public void OnSwitchView(InputAction.CallbackContext context)
    {
        if (context.performed)
            SetCameraMode(!isFirstPerson);
    }
}
