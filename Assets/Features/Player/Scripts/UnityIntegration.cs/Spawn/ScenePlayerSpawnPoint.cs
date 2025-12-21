using UnityEngine;

public class ScenePlayerSpawnPoint : MonoBehaviour, IPlayerSpawnProvider
{
    public int TeamId = 0;
    
    public bool TryGetSpawnPoint(out Vector3 position, out Quaternion rotation)
    {
        position = transform.position;
        rotation = transform.rotation;
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
#endif
}
