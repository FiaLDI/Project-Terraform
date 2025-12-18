using UnityEngine;
using System.Collections;

public class DebugRayDrawer : MonoBehaviour
{
    public static DebugRayDrawer Instance;

    public LineRenderer linePrefab;

    void Awake()
    {
        Instance = this;
    }

    public void DrawLaserRay(Vector3 start, Vector3 end, float duration = 0.1f)
    {
        if (linePrefab == null)
        {
            Debug.LogWarning("DebugRayDrawer: linePrefab NULL");
            return;
        }

        LineRenderer line = Instantiate(linePrefab);
        line.useWorldSpace = true;
        line.positionCount = 2;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        StartCoroutine(FadeAndDestroy(line, duration));
    }

    private IEnumerator FadeAndDestroy(LineRenderer lr, float duration)
    {
        float t = 0f;

        Color startColor = lr.startColor;
        Color endColor = lr.endColor;

        while (t < duration)
        {
            float a = 1f - (t / duration);

            Color c0 = startColor; c0.a = a;
            Color c1 = endColor;   c1.a = a;

            lr.startColor = c0;
            lr.endColor   = c1;

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(lr.gameObject);
    }
}
