using UnityEngine;

public sealed class ScenePlayerSpawnPoint : MonoBehaviour, IPlayerSpawnProvider
{
    private void Awake()
    {
        PlayerSpawnRegistry.I?.Register(this);
    }

    private void OnDestroy()
    {
        PlayerSpawnRegistry.I?.Unregister(this);
    }

    public bool TryGetSpawnPoint(out Vector3 pos, out Quaternion rot)
    {
        pos = transform.position;
        rot = transform.rotation;
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
