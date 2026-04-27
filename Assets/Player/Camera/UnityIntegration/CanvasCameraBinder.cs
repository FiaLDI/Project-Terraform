using UnityEngine;
using UnityEngine.UI;
using Features.Camera.UnityIntegration;

[RequireComponent(typeof(Canvas))]
public sealed class CanvasCameraBinder : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        ApplyCamera();
    }

    private void OnEnable()
    {
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged += HandleCameraChanged;

        ApplyCamera();
    }

    private void OnDisable()
    {
        if (CameraRegistry.Instance != null)
            CameraRegistry.Instance.OnCameraChanged -= HandleCameraChanged;
    }

    private void HandleCameraChanged(UnityEngine.Camera cam)
    {
        ApplyCamera(cam);
    }

    private void ApplyCamera()
    {
        var registry = CameraRegistry.Instance;
        ApplyCamera(registry != null ? registry.CurrentCamera : null);
    }

    private void ApplyCamera(UnityEngine.Camera cam)
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = cam;
    }
}
