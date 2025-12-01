using UnityEngine;
using System.Collections.Generic;

namespace Features.Pooling
{
    public class SmartPool : MonoBehaviour
    {
        public static SmartPool Instance { get; private set; }

        private readonly Dictionary<int, PoolGroup> groups = new();
        private Transform root;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            root = new GameObject("SmartPool").transform;
            DontDestroyOnLoad(root);
        }

        public PoolObject Get(int prefabIndex, GameObject prefab)
        {
            if (!groups.TryGetValue(prefabIndex, out var g))
            {
                g = new PoolGroup(prefab, root);
                groups[prefabIndex] = g;
            }

            return g.Get();
        }

        public void Release(PoolObject obj)
        {
            int index = obj.GetPrefabIndex();

            if (!groups.TryGetValue(index, out var g))
            {
                Destroy(obj.gameObject);
                return;
            }

            g.Release(obj);
        }
    }
}
