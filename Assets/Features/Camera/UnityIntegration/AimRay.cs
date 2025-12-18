using UnityEngine;

public static class AimRay
{
    public static Ray Create(Camera cam)
    {
        return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    }
}
