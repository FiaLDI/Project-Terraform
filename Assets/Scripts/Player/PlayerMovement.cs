using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputSystem_Actions _inputSystemAction;
    private Vector2 _moveInput;
    private CharacterController _characterController;

    [Header("Скорости")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _crouchSpeed = 1.5f;

    [Header("Прыжки и гравитация")]
    [SerializeField] private float _gravityScale = -9.81f;
    [SerializeField] private float _jumpForce = 5f;

    [Header("Приседание")]
    [SerializeField] private float _standHeight = 2.93f;
    [SerializeField] private float _crouchHeight = 1.2f;

    public bool IsCrouching { get; private set; } = false;

    private bool _isSprinting = false;
    private bool _isWalking = false;

    [Header("Управляемость в воздухе")]
    [SerializeField][Range(0f, 1f)] private float _airControl = 0.3f;

    private Vector3 _playerVelocity;

    private void Awake()
    {
        _inputSystemAction = new InputSystem_Actions();

        _inputSystemAction.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputSystemAction.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _inputSystemAction.Player.Jump.performed += ctx => JumpHandler();

        _inputSystemAction.Player.Sprint.performed += ctx => _isSprinting = true;
        _inputSystemAction.Player.Sprint.canceled += ctx => _isSprinting = false;

        _inputSystemAction.Player.Walk.performed += ctx => _isWalking = true;
        _inputSystemAction.Player.Walk.canceled += ctx => _isWalking = false;

        _inputSystemAction.Player.Crouch.performed += ctx => ToggleCrouch();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float currentSpeed;

        if (IsCrouching)
        {
            currentSpeed = _crouchSpeed;
        }
        else if (_isSprinting)
        {
            currentSpeed = _sprintSpeed;
        }
        else if (_isWalking)
        {
            currentSpeed = _walkSpeed;
        }
        else
        {
            currentSpeed = _movementSpeed;
        }

        bool isGrounded = _characterController.isGrounded;
        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        if (isGrounded)
        {
            _playerVelocity.x = moveDirection.x * currentSpeed;
            _playerVelocity.z = moveDirection.z * currentSpeed;
        }
        else
        {
            _playerVelocity += moveDirection * currentSpeed * _airControl * Time.deltaTime;
        }

        _playerVelocity.y += _gravityScale * Time.deltaTime;
        _characterController.Move(_playerVelocity * Time.deltaTime);
    }

    private void JumpHandler()
    {
        if (_characterController.isGrounded && !IsCrouching)
        {
            _playerVelocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravityScale);
        }
    }

    private void ToggleCrouch()
    {
        if (IsCrouching)
        {
            // Выходим из приседа
            if (CanStandUp())
            {
                IsCrouching = false;
                _characterController.height = _standHeight;
                _characterController.center = new Vector3(0, _standHeight / 2f, 0);
            }
        }
        else
        {
            
            IsCrouching = true;
            _characterController.height = _crouchHeight;
            _characterController.center = new Vector3(0, _crouchHeight / 2f, 0);
        }
    }

    private bool CanStandUp()
    {
        RaycastHit hit;
        float checkDistance = _standHeight - _crouchHeight;
        Vector3 start = transform.position + Vector3.up * _crouchHeight;
        return !Physics.SphereCast(start, _characterController.radius, Vector3.up, out hit, checkDistance);
    }

    private void OnEnable() => _inputSystemAction.Enable();
    private void OnDisable() => _inputSystemAction.Disable();
}
