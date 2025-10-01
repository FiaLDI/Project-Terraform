using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Основные ссылки")]
    [SerializeField] private Transform playerCamera;   // Камера игрока (Main Camera)
    [SerializeField] private Transform playerBody;     // Игрок (вращение по Y)
    [SerializeField] private Transform cameraPivot;    // Pivot для TPS (точка над плечами/головой)

    [Header("Режимы камеры")]
    public bool isFirstPerson = true; // текущий режим
    [SerializeField] private Vector3 fpsOffset = new Vector3(0f, 0.6f, 0f); // позиция FPS-камеры относительно тела
    [SerializeField] private float tpsCameraDistance = 3f; // расстояние от pivot в TPS
    [SerializeField] private float switchSpeed = 5f; // скорость плавного перемещения (TPS и коллизии)

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
    private bool justSwitchedView = false; // флаг мгновенной смены вида

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        // Подписка на ввод мыши
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Переключение вида (клавиша V)
        inputActions.Player.SwitchView.performed += ctx =>
        {
            isFirstPerson = !isFirstPerson;
            justSwitchedView = true;
        };

        // Настройки курсора
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
        // Горизонтальное вращение игрока (влево/вправо)
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);

        // Вертикальное вращение камеры (вверх/вниз)
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        if (isFirstPerson)
        {
            // FPS: вращаем камеру по X
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
        else
        {
            // TPS: вращаем pivot (ось X)
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleCameraPosition()
    {
        if (isFirstPerson)
        {
            // FPS: камера у головы игрока
            Vector3 targetPos = playerBody.position + fpsOffset;

            if (justSwitchedView)
            {
                playerCamera.position = targetPos; // мгновенно
                justSwitchedView = false;
            }
            else
            {
                playerCamera.position = Vector3.Lerp(playerCamera.position, targetPos, Time.deltaTime * switchSpeed);
            }
        }
        else
        {
            // TPS: направление от pivot назад
            Vector3 pivotPos = cameraPivot.position;
            Vector3 direction = -cameraPivot.forward;
            Vector3 targetPos = pivotPos + direction * tpsCameraDistance;

            // Проверка коллизий
            if (Physics.SphereCast(pivotPos, cameraCollisionRadius, direction, out RaycastHit hit, tpsCameraDistance, cameraCollisionMask))
            {
                float adjustedDistance = Mathf.Max(hit.distance - 0.1f, minCameraDistance);
                targetPos = pivotPos + direction * adjustedDistance;
            }

            if (justSwitchedView)
            {
                playerCamera.position = targetPos; // мгновенно
                justSwitchedView = false;
            }
            else
            {
                playerCamera.position = Vector3.Lerp(playerCamera.position, targetPos, Time.deltaTime * switchSpeed);
            }

            // Камера смотрит туда же, куда pivot
            playerCamera.rotation = cameraPivot.rotation;
        }
    }
}
