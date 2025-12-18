using UnityEngine;

public class GroundSnapProcessor : MonoBehaviour
{
    private void Update()
    {
        // Проходим всех PendingGroundSnap
        var snaps = FindObjectsOfType<PendingGroundSnap>();

        foreach (var s in snaps)
        {
            s.delay -= Time.deltaTime;

            if (s.delay <= 0f)
            {
                Transform t = s.transform;

                if (GroundSnapUtility.TrySnapWithNormal(
                        t.position,
                        out var snappedPos,
                        out var snappedRot,
                        out _))
                {
                    t.position = snappedPos;
                    t.rotation = snappedRot;
                }

                Object.Destroy(s); // удаляем компонент
            }
        }
    }
}
