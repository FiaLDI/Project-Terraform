using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    [Header("ADS Settings")]
    [SerializeField] private Transform adsPoint; // �����, ���� ������������ ������ ��� ������������
    [SerializeField] private float adsSpeed = 8f; // �������� �������� � ����� ������������

    [Header("Sway & Bob Settings")]
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swaySmoothness = 6f;
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Wall Collision")]
    [SerializeField] private LayerMask collisionLayerMask; // ����, ������� ��������� �������
    [SerializeField] private float collisionPushDistance = 0.5f; // ��������� ������ ���������� ������
    [SerializeField] private float weaponLength = 0.5f; // ����� ������ �� ������

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 targetPosition;
    private Camera playerCamera;
    private bool isInitialized = false;


    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Rigidbody playerRigidbody;
    // ���� ����� ����� ���������� ����� ��� �������� ������ ������
    public void Initialize(Camera cam, Rigidbody playerRb, Transform aimDownSightsPoint)
    {
        playerCamera = cam;
        playerRigidbody = playerRb;
        adsPoint = aimDownSightsPoint; // ��������� ���������� �����

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

        // ������ ��������� ��� ��������� � ������� � ��������
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * adsSpeed * 2);
    }

    private void HandleADS()
    {
        bool isAiming = Input.GetMouseButton(1); // ���������, ������ �� ���

        if (isAiming)
        {
            // ������� ������� - ����� ������������
            targetPosition = adsPoint.localPosition;
        }
        else
        {
            // ������� ������� - �������� ��������� "�� �����"
            targetPosition = originalPosition;
        }
    }

    private void HandleSwayAndBob()
    {
        // ����������� �� ���� (Sway)
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;

        // ������ ������������ ������
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation * originalRotation, Time.deltaTime * swaySmoothness);

        // ����������� �� ������ 
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
        // ������� ��� �� ������ ������
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, weaponLength, collisionLayerMask))
        {
            // ���� ��� ����� � �����, ���������� ������ �����
            float pushBack = weaponLength - hit.distance;
            targetPosition -= new Vector3(0, 0, collisionPushDistance * pushBack);
        }
    }
}