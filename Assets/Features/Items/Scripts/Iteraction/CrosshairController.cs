using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Image crosshair;
    public Camera cam;
    public float rayDistance = 3f;
    public LayerMask interactMask;

    public Color normalColor = Color.white;
    public Color interactColor = Color.cyan;

    void Update()
    {
        if (crosshair == null || cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactMask))
        {
            var interact = hit.collider.GetComponentInParent<IInteractable>();
            if (interact != null)
            {
                crosshair.color = interactColor;
                return;
            }
        }

        crosshair.color = normalColor;
    }
}
