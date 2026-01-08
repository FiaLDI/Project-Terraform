using UnityEngine;
using Features.Interaction.Domain;
using Features.Interaction.UnityIntegration;

[DefaultExecutionOrder(-500)]
public class CameraRayProvider : MonoBehaviour, IInteractionRayProvider
{
    [SerializeField] private float maxDistance = 3f;
    public float MaxDistance => maxDistance;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CameraRayProvider] Camera component NOT FOUND");
            enabled = false;
            return;
        }
        
            InteractionServiceProvider.Init(this);
    }

    public Ray GetRay()
    {
        return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
    }
}
