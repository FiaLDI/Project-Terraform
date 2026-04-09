using UnityEngine;
using Features.Player.UnityIntegration;
using Features.Camera.UnityIntegration;

public sealed class PlayerCameraController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform fpsPoint;
    [SerializeField] private Transform visualRoot;

    [Header("Settings")]
    private float sensitivity = 1.5f;
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;
    [SerializeField] private LayerMask fpsMask;
    [SerializeField] private LayerMask tpsMask;

    [Header("TPS")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0, 0);

    private Transform cam;

    private float yaw;
    private float pitch;

    private bool isFPS = true;
    private bool isLocal;
    private bool isAiming;

    private MovementInputHandler inputHandler;

    private void Awake()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        
        ApplySensitivity();
    }

    public void InjectInput(MovementInputHandler inputHandler)
    {
        this.inputHandler = inputHandler;
    }

    private void OnCameraChanged(Camera cam)
    {
        this.cam = cam != null ? cam.transform : null;
    }

    private void OnDisable()
    {
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged -= OnCameraChanged;
    }

    public void SetLocal(bool value)
    {
        isLocal = value;
        enabled = value;

        if (!value)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CameraRegistry.Instance?.InitializeFPS();
        CameraRegistry.Instance?.SetFPSVisible(isFPS);
        CameraRegistry.Instance.OnCameraChanged += OnCameraChanged;
        ResolveCamera();
    }

    // ================= INPUT =================

    public void SetLookInput(Vector2 input)
    {
        if (!isLocal) return;

        float sens = sensitivity * 100f * Time.deltaTime;

        yaw += input.x * sens;
        pitch -= input.y * sens;

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (inputHandler != null)
        {
            inputHandler.SetYaw(yaw);
            inputHandler.SetPitch(pitch);
        }
    }

    public void SwitchView()
    {
        isFPS = !isFPS;
        CameraRegistry.Instance?.SetFPSVisible(isFPS);
    }

    public bool IsFPS()
    {
        return isFPS;
    }

    public void SetAiming(bool value)
    {
        isAiming = value;
    }

    public void ResolveCamera()
    {
        var unityCam = CameraRegistry.Instance?.CurrentCamera;
        cam = unityCam != null ? unityCam.transform : null;
    }

    public void SetHead(Transform head)
    {
        // больше не нужен, но оставим для совместимости
    }

    public void RefreshSensitivity()
    {
        ApplySensitivity();
    }

    private void ApplySensitivity()
    {
        sensitivity = SettingsStorage.Sensitivity;
    }

    // ================= UPDATE =================

    private void LateUpdate()
    {
        if (!isLocal || cam == null)
            return;

        if (isFPS)
            UpdateFPS();
        else
            UpdateTPS();
        
        var unityCamera = CameraRegistry.Instance?.CurrentCamera;

        if (unityCamera != null)
        {
            unityCamera.cullingMask = isFPS ? fpsMask : tpsMask;
        }
    }

    private void UpdateFPS()
    {
        cam.position = fpsPoint.position;
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateTPS()
    {
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 pivot = cameraPivot.position;

        float finalDistance = isAiming ? 2f : distance;

        Vector3 back = cameraPivot.forward * -finalDistance;

        Vector3 offsetWorld =
            cameraPivot.right * shoulderOffset.x +
            cameraPivot.up    * shoulderOffset.y;

        Vector3 targetPos =
            pivot + offsetWorld + back;

        cam.position = targetPos;
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
