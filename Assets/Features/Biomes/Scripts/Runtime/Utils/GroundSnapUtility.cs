using UnityEngine;

public static class GroundSnapUtility
{
    public static bool TrySnapWithNormal(
        Vector3 pos,
        out Vector3 snappedPos,
        out Quaternion rotation,
        out float slopeAngle,
        float maxDistance = 200f)
    {
        Ray ray = new Ray(pos + Vector3.up * maxDistance, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance * 2f))
        {
            snappedPos = hit.point;
            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return true;
        }

        snappedPos = pos;
        rotation = Quaternion.identity;
        slopeAngle = 0f;
        return false;
    }
}
