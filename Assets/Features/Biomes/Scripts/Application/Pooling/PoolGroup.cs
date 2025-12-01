using System.Collections.Generic;
using UnityEngine;

namespace Features.Pooling
{
    public class PoolGroup
    {
        private readonly GameObject prefab;
        private readonly Transform root;
        private readonly Stack<PoolObject> pool = new();

        public PoolGroup(GameObject prefab, Transform root)
        {
            this.prefab = prefab;
            this.root   = new GameObject(prefab.name + "_Pool").transform;
            this.root.SetParent(root, false);
        }

        public PoolObject Get()
        {
            PoolObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Pop();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(prefab).AddComponent<PoolObject>();
            }

            return obj;
        }

        public void Release(PoolObject obj)
        {
            obj.transform.SetParent(root, false);
            obj.gameObject.SetActive(false);
            pool.Push(obj);
        }
    }
}
