using FishNet.Object;
using UnityEngine;

public class ResourceNodeSpawner : NetworkBehaviour
{
    [Header("Prefab")]
    public NetworkObject resourceNodePrefab;

    [Header("Spawn Settings")]
    public int count = 10;
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);
    public float yOffset = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[ResourceNodeSpawner] OnStartServer", this);

        if (resourceNodePrefab == null)
        {
            Debug.LogError("[ResourceNodeSpawner] resourceNodePrefab is NULL", this);
            return;
        }

        SpawnNodes();
    }

    [Server]
    private void SpawnNodes()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetRandomPointInArea();
            Quaternion rot = Quaternion.identity;

            NetworkObject nob = Instantiate(resourceNodePrefab, pos, rot);
            Debug.Log($"[ResourceNodeSpawner] Instantiate {nob.name} at {pos}", nob);

            Spawn(nob);
            Debug.Log($"[ResourceNodeSpawner] Spawned NO id={nob.ObjectId}", nob);
        }
    }

    private Vector3 GetRandomPointInArea()
    {
        var center = transform.position;
        float x = center.x + Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float z = center.z + Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f);
        float y = center.y + yOffset;

        return new Vector3(x, y, z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + new Vector3(0f, yOffset, 0f), areaSize);
    }
}
