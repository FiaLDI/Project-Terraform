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

    [Header("Aim")]
    [SerializeField] private float normalFov = 70f;
    [SerializeField] private float fpsAimFov = 52f;
    [SerializeField] private float tpsAimFov = 58f;
    [SerializeField] private float aimSmooth = 14f;
    [SerializeField] private float tpsAimDistance = 1.35f;
    [SerializeField] private Vector3 tpsAimShoulderOffset = new Vector3(0.3f, 0.1f, 0.15f);
    [SerializeField] private float tpsAimPivotHeight = 0.18f;
    [SerializeField] private float tpsAimForwardOffset = 0.35f;
    [SerializeField] private Vector3 fpsAimOffset = new Vector3(0f, -0.03f, 0.05f);

    private Transform cam;

    private float yaw;
    private float pitch;
    private float currentCrouchOffset;
    private float currentAimBlend;

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
        CameraRegistry.Instance?.SetFPSVisible(false);
        CameraRegistry.Instance.OnCameraChanged += OnCameraChanged;
        ResolveCamera();
        GetComponent<Features.Equipment.UnityIntegration.EquipmentManager>()?.RefreshViewModelVisibility();
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
        GetComponent<Features.Equipment.UnityIntegration.EquipmentManager>()?.RefreshViewModelVisibility();
    }

    public bool IsFPS()
    {
        return isFPS;
    }

    public float CurrentPitch => pitch;
    public float CurrentYaw => yaw;

    public void SetWeaponPose(int pose)
    {
        CameraRegistry.Instance?.SetWeaponPose(Mathf.Clamp(pose, 0, 2));
    }

    public void SetAiming(bool value)
    {
        isAiming = value;
    }

    public void SetLookEnabled(bool value)
    {
        isLookEnabled = value;

        if (!value)
            isAiming = false;
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

        currentAimBlend = Mathf.Lerp(
            currentAimBlend,
            isAiming ? 1f : 0f,
            1f - Mathf.Exp(-Mathf.Max(1f, aimSmooth) * Time.deltaTime)
        );

        if (isFPS)
            UpdateFPS();
        else
            UpdateTPS();

        var unityCamera = CameraRegistry.Instance?.CurrentCamera;
        if (unityCamera != null)
        {
            unityCamera.cullingMask = isFPS ? fpsMask : tpsMask;
            unityCamera.fieldOfView = Mathf.Lerp(
                normalFov,
                isFPS ? fpsAimFov : tpsAimFov,
                currentAimBlend
            );
        }
    }

    private void UpdateFPS()
    {
        Vector3 crouchOffset = Vector3.up * GetCurrentCrouchOffset();
        Vector3 aimOffsetWorld =
            fpsPoint.right * fpsAimOffset.x +
            fpsPoint.up * fpsAimOffset.y +
            fpsPoint.forward * fpsAimOffset.z;

        cam.position = fpsPoint.position + crouchOffset + aimOffsetWorld * currentAimBlend;
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
            Vector3.up * (GetCurrentCrouchOffset() + Mathf.Lerp(tpsPivotHeight, tpsAimPivotHeight, currentAimBlend)) +
            planarForward * Mathf.Lerp(tpsPivotForwardOffset, tpsAimForwardOffset, currentAimBlend);

        float finalDistance = Mathf.Lerp(distance, tpsAimDistance, currentAimBlend);

        Vector3 back = cameraPivot.forward * -finalDistance;

        Vector3 blendedShoulderOffset = Vector3.Lerp(shoulderOffset, tpsAimShoulderOffset, currentAimBlend);

        Vector3 offsetWorld =
            cameraPivot.right * blendedShoulderOffset.x +
            Vector3.up * blendedShoulderOffset.y +
            planarForward * blendedShoulderOffset.z;

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
