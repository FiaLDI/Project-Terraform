using UnityEngine;
using Features.Interaction.Domain;
using Features.Camera.UnityIntegration;

namespace Features.Interaction.UnityIntegration
{
    public class CameraRayProvider : MonoBehaviour, IInteractionRayProvider
    {
        private UnityEngine.Camera cam;

        [SerializeField] private float maxDistance = 4f;
        public float MaxDistance => maxDistance;

        private void OnEnable()
        {
            if (CameraRegistry.Instance != null)
            {
                CameraRegistry.Instance.OnCameraChanged += HandleCam;

                if (CameraRegistry.Instance.CurrentCamera != null)
                    cam = CameraRegistry.Instance.CurrentCamera;
            }

            InteractionServiceProvider.Init(this);
        }

        private void OnDisable()
        {
            if (CameraRegistry.Instance != null)
                CameraRegistry.Instance.OnCameraChanged -= HandleCam;
        }

        private void HandleCam(UnityEngine.Camera newCam)
        {
            cam = newCam;
        }

        public Ray GetRay()
        {
            // ❗ Если камеры нет — НЕ ВОЗВРАЩАЕМ ФЕЙК
            if (cam == null)
                throw new System.InvalidOperationException(
                    "[CameraRayProvider] Camera is not registered yet"
                );

            // Луч строго из центра экрана
            return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        }
    }
}
