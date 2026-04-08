using System.Collections.Generic;
using UnityEngine;

public sealed class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject obj;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);

            var pooled = obj.GetComponent<PooledProjectile>();
            if (pooled == null)
                pooled = obj.AddComponent<PooledProjectile>();

            pooled.Init(this, prefab);
        }

        return obj;
    }

    public void Release(GameObject obj, GameObject prefab)
    {
        obj.SetActive(false);

        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        queue.Enqueue(obj);
    }
}
