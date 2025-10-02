using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    public float SneakSpeed = 2f;
    public float JumpHeight = 2f;
    public float Gravity = -9.81f;

    [Header("Crouch Settings")]
    public float CrouchHeight = 1f;
    public float StandHeight = 2f;
    public float CrouchSpeed = 2.5f;
    public float CrouchTransitionSpeed = 10f;

    [Header("Camera Settings")]
    [Tooltip("Базовая чувствительность мыши. Подбирается под комфорт.")]
    public float MouseSensitivity = 5f;
    public float MaxLookAngle = 80f;             
    public float ThirdPersonMaxLookAngle = 45f; 
    [Tooltip("Плавность вращения камеры 3-го лица")]
    public float ThirdPersonSmooth = 10f;

    [Header("Cameras")]
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;
    public Transform ThirdPersonPivot;
    public float ThirdPersonDistance = 3f;
    public float MinDistance = 0.5f;
    public LayerMask CollisionMask;

    private CharacterController _controller;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isSneaking;
    private bool _isCrouching;
    private bool _jumpPressed = false;
    private float _xRotation = 0f;
    private bool _isFirstPerson = true;
    private float _currentDistance;

    private float _currentXRotation;
    private float _xRotationVelocity;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        _controller.height = StandHeight;
        _controller.center = new Vector3(0, StandHeight / 2f, 0);

        SetCameraMode(true);
        _currentDistance = ThirdPersonDistance;

        Vector3 camPos = FirstPersonCamera.transform.localPosition;
        camPos.y = StandHeight - 0.2f;
        FirstPersonCamera.transform.localPosition = camPos;

        Vector3 pivotPos = ThirdPersonPivot.localPosition;
        pivotPos.y = StandHeight - 0.2f;
        ThirdPersonPivot.localPosition = pivotPos;
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleCrouch();
    }

    private void HandleMovement()
    {
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        float speed = WalkSpeed;

        if (_isCrouching)
            speed = CrouchSpeed;
        else if (_isSneaking)
            speed = SneakSpeed;
        else if (_isRunning)
            speed = RunSpeed;

        _controller.Move(move * speed * Time.deltaTime);

        if (_jumpPressed && _isGrounded && !_isCrouching)
        {
            _velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            _jumpPressed = false;
        }

        _velocity.y += Gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleCamera()
    {
        float mouseX = _lookInput.x * MouseSensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * MouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;

        if (_isFirstPerson)
        {
            _xRotation = Mathf.Clamp(_xRotation, -MaxLookAngle, MaxLookAngle);
            FirstPersonCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
        else
        {
            _xRotation = Mathf.Clamp(_xRotation, -ThirdPersonMaxLookAngle, ThirdPersonMaxLookAngle);

            _currentXRotation = Mathf.SmoothDamp(_currentXRotation, _xRotation, ref _xRotationVelocity, 1f / ThirdPersonSmooth);

            ThirdPersonPivot.localRotation = Quaternion.Euler(_currentXRotation, 0f, 0f);
            HandleThirdPersonCollision();
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleThirdPersonCollision()
    {
        Vector3 pivotPos = ThirdPersonPivot.position;
        Vector3 desiredCameraPos = pivotPos - ThirdPersonPivot.forward * ThirdPersonDistance;

        Vector3 dir = desiredCameraPos - pivotPos;
        dir.Normalize();

        if (Physics.SphereCast(pivotPos, 0.3f, dir, out RaycastHit hit, ThirdPersonDistance, CollisionMask))
        {
            _currentDistance = Mathf.Clamp(hit.distance - 0.1f, MinDistance, ThirdPersonDistance);
        }
        else
        {
            _currentDistance = ThirdPersonDistance;
        }

        ThirdPersonCamera.transform.position = pivotPos - ThirdPersonPivot.forward * _currentDistance;
        ThirdPersonCamera.transform.rotation = ThirdPersonPivot.rotation;
    }

    private void HandleCrouch()
    {
        float targetHeight = _isCrouching ? CrouchHeight : StandHeight;

        _controller.height = Mathf.Lerp(_controller.height, targetHeight, Time.deltaTime * CrouchTransitionSpeed);
        _controller.center = new Vector3(0, _controller.height / 2f, 0);

        float targetCamY = _isCrouching ? CrouchHeight - 0.2f : StandHeight - 0.2f;

        if (_isFirstPerson)
        {
            Vector3 camPos = FirstPersonCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * CrouchTransitionSpeed);
            FirstPersonCamera.transform.localPosition = camPos;
        }
        else
        {
            Vector3 pivotPos = ThirdPersonPivot.localPosition;
            pivotPos.y = Mathf.Lerp(pivotPos.y, targetCamY, Time.deltaTime * CrouchTransitionSpeed);
            ThirdPersonPivot.localPosition = pivotPos;
        }
    }

    private void SetCameraMode(bool firstPerson)
    {
        _isFirstPerson = firstPerson;
        FirstPersonCamera.enabled = firstPerson;
        ThirdPersonCamera.enabled = !firstPerson;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMovement(InputAction.CallbackContext context) =>
        _moveInput = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context) =>
        _lookInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            _jumpPressed = true;
    }

    public void OnRun(InputAction.CallbackContext context) =>
        _isRunning = context.ReadValueAsButton();

    public void OnSneak(InputAction.CallbackContext context) =>
        _isSneaking = context.ReadValueAsButton();

    public void OnCrouch(InputAction.CallbackContext context) =>
        _isCrouching = context.ReadValueAsButton();

    public void OnSwitchView(InputAction.CallbackContext context)
    {
        if (context.performed)
            SetCameraMode(!_isFirstPerson);
    }
}
