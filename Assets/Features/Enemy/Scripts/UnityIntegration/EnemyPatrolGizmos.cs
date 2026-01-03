using UnityEngine;

public sealed class EnemyPatrolGizmos : MonoBehaviour
{
    [SerializeField] private Transform patrolRoot;
    [SerializeField] private Color color = Color.cyan;

    private void OnDrawGizmos()
    {
        if (patrolRoot == null || patrolRoot.childCount == 0)
            return;

        Gizmos.color = color;

        Vector3 prev = patrolRoot.GetChild(0).position;

        for (int i = 1; i < patrolRoot.childCount; i++)
        {
            Vector3 curr = patrolRoot.GetChild(i).position;
            Gizmos.DrawLine(prev, curr);
            Gizmos.DrawSphere(curr, 0.15f);
            prev = curr;
        }

        // замкнуть маршрут
        Gizmos.DrawLine(
            prev,
            patrolRoot.GetChild(0).position
        );
    }
}
