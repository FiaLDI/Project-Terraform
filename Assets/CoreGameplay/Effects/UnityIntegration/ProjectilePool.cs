using System.Collections.Generic;
using UnityEngine;

public sealed class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
            return null;

        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject obj;

        while (queue.Count > 0)
        {
            obj = queue.Dequeue();
            if (obj == null)
                continue;

            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
            return obj;
        }

        obj = Instantiate(prefab, pos, rot);

        var pooled = obj.GetComponent<PooledProjectile>();
        if (pooled == null)
            pooled = obj.AddComponent<PooledProjectile>();

        pooled.Init(this, prefab);

        return obj;
    }

    public void Release(GameObject obj, GameObject prefab)
    {
        if (obj == null || prefab == null)
            return;

        obj.SetActive(false);

        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        queue.Enqueue(obj);
    }
}
