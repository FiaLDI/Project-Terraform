// Assets/Features/Camera/Scripts/UnityIntegration/CameraAnchor.cs
using UnityEngine;

namespace Features.Camera.UnityIntegration
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraAnchor : MonoBehaviour
    {
        private UnityEngine.Camera cam;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            CameraRegistry.Instance?.RegisterCamera(cam);
        }

        private void OnDestroy()
        {
            CameraRegistry.Instance?.UnregisterCamera(cam);
        }
    }
}
