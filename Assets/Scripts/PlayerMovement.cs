using UnityEngine;
using System;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputSystem_Actions _inputSystemAction;
    private Vector2 _moveInput;
    private CharacterController _characterController;

    //Настройки движения через WASD
    [SerializeField] private Vector3 _moveDirection;
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _walkSpeed = 2;
    [SerializeField] private float _sprintSpeed = 8;
    [SerializeField] private float _verticalVelocity;
    [SerializeField] private float _gravityScale = -9.81f;
    [SerializeField] private float _jumpForce = 5;
    //[SerializeField] private float _sprintForce = 10;
    private bool _isSprinting = false;
    private bool _isWalking = false;



    [Header("Настройки камеры")]
    [SerializeField] private Transform _playerCamera; // Камера игрока
    [SerializeField] private float _mouseSensitivity = 50f;
    [SerializeField][Range(-90f, 0f)] private float _minPitch = -60f; // Ограничение взгляда вниз
    [SerializeField][Range(0f, 90f)] private float _maxPitch = 80f;  // Ограничение взгляда вверх

    [Header("Управляемость в воздухе")]
    [SerializeField][Range(0f, 1f)] private float _airControl = 0.3f; // Управляемость движением во время полёта (0 = нет, 1 = полный контроль)


    private Vector2 _lookInput;
    private float _cameraPitch = 0f; // Текущий вертикальный угол камеры
    private Vector3 _playerVelocity; // Вектор, хранящий реальную скорость игрока, включая гравитацию

    private void Awake()
    {
        // Создание экземпляра класса Input Actions
        _inputSystemAction = new InputSystem_Actions();

        // Подписка на событие "Jump"
        _inputSystemAction.Player.Jump.performed += context => JumpHandler();

        // Подписка на событие "Move" (когда есть ввод)
        _inputSystemAction.Player.Move.performed += context => _moveInput = context.ReadValue<Vector2>();

        // Подписка на событие "Move" (когда ввод прекратился)
        _inputSystemAction.Player.Move.canceled += context => _moveInput = Vector2.zero;

        

        //Подписка на события мыши
        _inputSystemAction.Player.Look.performed += context => _lookInput = context.ReadValue<Vector2>();
        _inputSystemAction.Player.Look.canceled += context => _lookInput = Vector2.zero;

        //Подписка на нажатие клавиш CTRL и SHIFT
        _inputSystemAction.Player.Sprint.performed += context => _isSprinting = true;
        _inputSystemAction.Player.Sprint.canceled += context => _isSprinting = false;
        _inputSystemAction.Player.Walk.performed += context => _isWalking = true;
        _inputSystemAction.Player.Walk.canceled += context => _isWalking = false;

        //Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
    }
    private void LateUpdate()
    {
        // Логику камеры лучше выполнять в LateUpdate, чтобы избежать дерганий
        HandleLook();
    }

    private void HandleMovement()
    {

        float currentSpeed;
        if (_isSprinting)
        {
            currentSpeed = _sprintSpeed;
        }
        else if (_isWalking)
        {
            currentSpeed = _walkSpeed;
        }
        else
        {
            currentSpeed = _movementSpeed; // Скорость по умолчанию
        }

        bool isGrounded = _characterController.isGrounded;
        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        if (isGrounded)
        {
            // Используем currentSpeed вместо постоянного значения
            _playerVelocity.x = moveDirection.x * currentSpeed;
            _playerVelocity.z = moveDirection.z * currentSpeed;
        }
        else
        {
            // Также используем currentSpeed и здесь
            _playerVelocity += moveDirection * currentSpeed * _airControl * Time.deltaTime;
        }

        _playerVelocity.y += _gravityScale * Time.deltaTime;
        _characterController.Move(_playerVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // 1. Горизонтальное вращение (вокруг оси Y)
        // Вращаем весь объект Player, чтобы тело и камера поворачивались вместе
        float mouseX = _lookInput.x * _mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // 2. Вертикальное вращение (вокруг оси X)
        // Вращаем только камеру, чтобы тело не наклонялось
        float mouseY = _lookInput.y * _mouseSensitivity * Time.deltaTime;
        _cameraPitch -= mouseY; // Инвертируем, т.к. движение мыши вверх должно давать отрицательный угол

        // 3. Ограничение вертикального угла
        _cameraPitch = Mathf.Clamp(_cameraPitch, _minPitch, _maxPitch);

        // Применяем вращение к камере
        _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }


    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void JumpHandler()
    {
        if (_characterController.isGrounded)
        {
            _playerVelocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravityScale);
        }
    }

    private void OnEnable()
    {
        _inputSystemAction.Enable();
    }

    private void OnDisable()
    {
        _inputSystemAction.Disable();
    }
}
