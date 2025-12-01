using UnityEngine;
using System.Collections.Generic;

public class ResourceVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    public float radius = 30f;
    public float pointSize = 0.5f;
    public bool showColorsByCluster = true;

    [Header("Debug Output")]
    public bool logCountToConsole = true;
    private int lastCount = -1;

    public static readonly List<(Vector3 pos, ResourceClusterType type)> resourcePositions 
        = new List<(Vector3 pos, ResourceClusterType type)>();

    private void OnDrawGizmos()
    {
        if (resourcePositions.Count == 0)
            return;

        int countInRadius = 0;

        foreach (var item in resourcePositions)
        {
            float dist = Vector3.Distance(transform.position, item.pos);

            if (dist <= radius)
            {
                countInRadius++;

                Gizmos.color = showColorsByCluster ? GetColorByType(item.type) : Color.green;
                Gizmos.DrawSphere(item.pos, pointSize);
            }
        }

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, radius);

        if (logCountToConsole && countInRadius != lastCount)
        {
            Debug.Log($"[ResourceVisualizer] Resources nearby: {countInRadius}");
            lastCount = countInRadius;
        }
    }


    private Color GetColorByType(ResourceClusterType type)
    {
        switch (type)
        {
            case ResourceClusterType.Single:
                return Color.green;
            case ResourceClusterType.CrystalVein:
                return Color.cyan;
            case ResourceClusterType.RoundCluster:
                return Color.yellow;
            case ResourceClusterType.VerticalStackNoise:
                return Color.magenta;
            default:
                return Color.white;
        }
    }
}
