using UnityEngine;
using Features.Camera.UnityIntegration;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraBinder : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();

        // ВАЖНО: ставим Screen Space - Camera
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    private void Start()
    {
        BindCamera();
    }

    private void OnEnable()
    {
        // Если камера сменится — обновим
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged += HandleCameraChanged;
    }

    private void OnDisable()
    {
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged -= HandleCameraChanged;
    }

    private void HandleCameraChanged(Camera cam)
    {
        canvas.worldCamera = cam;
    }

    private void BindCamera()
    {
        if (CameraRegistry.Instance == null) return;

        Camera cam = CameraRegistry.Instance.CurrentCamera;
        if (cam != null)
        {
            canvas.worldCamera = cam;
        }
        else
        {
            Debug.LogWarning("[CanvasCameraBinder] Camera is null on start!");
        }
    }
}
