using UnityEngine;

public static class GroundSnapUtility
{
    /// <summary>
    /// Делаем рейкаст вниз ИСКЛЮЧИТЕЛЬНО ради корректировки объектов,
    /// которые должны прилипать к земле. Используется только в редких местах.
    /// </summary>
    public static bool TrySnapWithNormal(
        Vector3 pos,
        out Vector3 snappedPos,
        out Quaternion rotation,
        out float slopeAngle,
        float maxDistance = 200f,
        int layerMask = Physics.DefaultRaycastLayers)
    {
        // точка старта — ВЫШЕ pos, чтобы всегда быть над объектами
        Vector3 origin = pos + Vector3.up * maxDistance;

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                maxDistance * 2f,
                layerMask,
                QueryTriggerInteraction.Ignore))
        {
            Vector3 normal = hit.normal;

            // защита
            if (!IsFinite(normal) || normal.sqrMagnitude < 0.0001f)
                normal = Vector3.up;

            snappedPos = hit.point;

            rotation = Quaternion.FromToRotation(Vector3.up, normal);
            slopeAngle = Vector3.Angle(normal, Vector3.up);

            return true;
        }

        // fallback
        snappedPos = pos;
        rotation = Quaternion.identity;
        slopeAngle = 0f;
        return false;
    }

    private static bool IsFinite(Vector3 v) =>
        !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
        !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
}
