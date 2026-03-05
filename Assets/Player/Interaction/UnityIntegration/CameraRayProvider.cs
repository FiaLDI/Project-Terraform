using UnityEngine;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;

[DefaultExecutionOrder(-500)]
public class CameraRayProvider : MonoBehaviour, IInteractionRayProvider
{
    [SerializeField] private float maxDistance = 3f;
    public float MaxDistance => maxDistance;

    private UnityEngine.Camera cam;

    private void Awake()
    {
        cam = GetComponent<UnityEngine.Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraRayProvider] Camera component NOT FOUND");
            enabled = false;
            return;
        }

        // 🔥 всегда сбрасываем перед инициализацией
        InteractionServiceProvider.Reset();
        InteractionServiceProvider.Init(this);
    }

    public Ray GetRay()
    {
        if (cam == null)
            return default;

        return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    }
}