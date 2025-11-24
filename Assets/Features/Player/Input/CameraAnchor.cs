using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
            CameraRegistry.I?.RegisterCamera(cam);
        
        Debug.Log(cam);
    }
}
