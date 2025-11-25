using UnityEngine;
//Устаревший скрипт
public class HeldItemController : MonoBehaviour
{
    [Header("ADS Settings")]
    [SerializeField] private Transform adsPoint; // Точка, куда перемещается оружие при прицеливании
    [SerializeField] private float adsSpeed = 8f; // Скорость перехода в режим прицеливания

    [Header("Sway & Bob Settings")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swaySmoothness = 6f;
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Wall Collision")]
    [SerializeField] private LayerMask collisionLayerMask; // Слои, которые считаются стенами
    [SerializeField] private float collisionPushDistance = 0.5f; // Насколько сильно отодвигать оружие
    [SerializeField] private float weaponLength = 0.5f; // Длина оружия от камеры

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 targetPosition;
    private Camera playerCamera;
    private bool isInitialized = false;


    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Rigidbody playerRigidbody;
    // Этот метод будет вызываться извне для передачи нужных ссылок
    public void Initialize(Camera cam, Rigidbody playerRb, Transform aimDownSightsPoint)
    {
        playerCamera = cam;
        playerRigidbody = playerRb;
        adsPoint = aimDownSightsPoint; // Назначаем переданную точку

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        isInitialized = true;
    }

    public void Initialize(Camera cam, Rigidbody playerRb)
    {
        playerCamera = cam;
        playerRigidbody = playerRb;

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        isInitialized = true;
    }
    private void Update()
    {
        if (!isInitialized) return;

        HandleADS();
        HandleSwayAndBob();
        HandleWallCollision();

        // Плавно применяем все изменения к позиции и повороту
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * adsSpeed * 2);
    }

    private void HandleADS()
    {
        bool isAiming = Input.GetMouseButton(1); // Проверяем, зажата ли ПКМ

        if (isAiming)
        {
            // Целевая позиция - точка прицеливания
            targetPosition = adsPoint.localPosition;
        }
        else
        {
            // Целевая позиция - исходное положение "от бедра"
            targetPosition = originalPosition;
        }
    }

    private void HandleSwayAndBob()
    {
        // Покачивание от мыши (Sway)
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;

        // Плавно поворачиваем оружие
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation * originalRotation, Time.deltaTime * swaySmoothness);

        // Покачивание от ходьбы 
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmount;
            targetPosition += new Vector3(0, bobOffset, 0);
        }
    }

    private void HandleWallCollision()
    {
        RaycastHit hit;
        // Пускаем луч из камеры вперед
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, weaponLength, collisionLayerMask))
        {
            // Если луч попал в стену, отодвигаем оружие назад
            float pushBack = weaponLength - hit.distance;
            targetPosition -= new Vector3(0, 0, collisionPushDistance * pushBack);
        }
    }
}