using UnityEngine;
using Features.Player.UnityIntegration;
using Features.Camera.UnityIntegration;

public sealed class PlayerCameraController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform fpsPoint;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private DeterministicMovement movement;

    [Header("Settings")]
    private float sensitivity = 1.5f;
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;
    [SerializeField] private LayerMask fpsMask;
    [SerializeField] private LayerMask tpsMask;

    [Header("TPS")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0, 0);
    [SerializeField] private float tpsPivotHeight = 0.35f;
    [SerializeField] private float tpsPivotForwardOffset = 0.15f;
    [SerializeField] private LayerMask tpsCollisionMask = ~0;
    [SerializeField] private float tpsCollisionRadius = 0.2f;
    [SerializeField] private float tpsMinDistance = 0.35f;

    [Header("Crouch Camera")]
    [SerializeField] private float crouchCameraDrop = 0.4f;
    [SerializeField] private float crouchCameraSmooth = 12f;

    private Transform cam;

    private float yaw;
    private float pitch;
    private float currentCrouchOffset;

    private bool isFPS = true;
    private bool isLocal;
    private bool isAiming;
    private bool isLookEnabled = true;

    private MovementInputHandler inputHandler;
    private readonly RaycastHit[] tpsHits = new RaycastHit[8];

    private void Awake()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        if (movement == null)
            movement = GetComponent<DeterministicMovement>();

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

    public void SetLookInput(Vector2 input)
    {
        if (!isLocal || !isLookEnabled)
            return;

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
        if (!isLookEnabled)
            return;

        isFPS = !isFPS;
        CameraRegistry.Instance?.SetFPSVisible(isFPS);
    }

    public bool IsFPS()
    {
        return isFPS;
    }

    public float CurrentPitch => pitch;
    public float CurrentYaw => yaw;

    public void SetAiming(bool value)
    {
        isAiming = value;
    }

    public void SetLookEnabled(bool value)
    {
        isLookEnabled = value;
    }

    public void ResolveCamera()
    {
        var unityCam = CameraRegistry.Instance?.CurrentCamera;
        cam = unityCam != null ? unityCam.transform : null;
    }

    public void SetHead(Transform head)
    {
        if (head == null)
            return;

        HeadPitchController headPitch = head.GetComponent<HeadPitchController>();
        if (headPitch == null)
            headPitch = head.gameObject.AddComponent<HeadPitchController>();

        headPitch.BindCamera(this);
    }

    public void RefreshSensitivity()
    {
        ApplySensitivity();
    }

    private void ApplySensitivity()
    {
        sensitivity = SettingsStorage.Sensitivity;
    }

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
            unityCamera.cullingMask = isFPS ? fpsMask : tpsMask;
    }

    private void UpdateFPS()
    {
        Vector3 crouchOffset = Vector3.up * GetCurrentCrouchOffset();
        cam.position = fpsPoint.position + crouchOffset;
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateTPS()
    {
        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 planarForward = Vector3.ProjectOnPlane(cameraPivot.forward, Vector3.up);
        if (planarForward.sqrMagnitude < 0.0001f)
            planarForward = transform.forward;
        else
            planarForward.Normalize();

        Vector3 pivot =
            cameraPivot.position +
            Vector3.up * (GetCurrentCrouchOffset() + tpsPivotHeight) +
            planarForward * tpsPivotForwardOffset;

        float finalDistance = isAiming ? 2f : distance;

        Vector3 back = cameraPivot.forward * -finalDistance;

        Vector3 offsetWorld =
            cameraPivot.right * shoulderOffset.x +
            Vector3.up * shoulderOffset.y +
            planarForward * shoulderOffset.z;

        Vector3 shoulderPivot = pivot + offsetWorld;
        float resolvedDistance = ResolveTpsDistance(shoulderPivot, back.normalized, finalDistance);
        Vector3 targetPos = shoulderPivot + back.normalized * resolvedDistance;

        cam.position = targetPos;
        cam.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private float GetCurrentCrouchOffset()
    {
        float targetOffset = 0f;
        if (movement != null && movement.IsCrouching)
            targetOffset = -Mathf.Abs(crouchCameraDrop);

        float lerpFactor = 1f - Mathf.Exp(-Mathf.Max(1f, crouchCameraSmooth) * Time.deltaTime);
        currentCrouchOffset = Mathf.Lerp(currentCrouchOffset, targetOffset, lerpFactor);
        return currentCrouchOffset;
    }

    private float ResolveTpsDistance(Vector3 origin, Vector3 direction, float desiredDistance)
    {
        if (desiredDistance <= tpsMinDistance)
            return desiredDistance;

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            tpsCollisionRadius,
            direction,
            tpsHits,
            desiredDistance,
            tpsCollisionMask,
            QueryTriggerInteraction.Ignore
        );

        float resolvedDistance = desiredDistance;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = tpsHits[i];
            if (hit.collider == null)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            resolvedDistance = Mathf.Min(
                resolvedDistance,
                Mathf.Max(hit.distance - tpsCollisionRadius, tpsMinDistance)
            );
        }

        return resolvedDistance;
    }
}
