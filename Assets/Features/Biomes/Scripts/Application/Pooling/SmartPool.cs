using System.Collections.Generic;
using UnityEngine;

namespace Features.Pooling
{
    public class SmartPool : MonoBehaviour
    {
        public static SmartPool Instance;

        [Header("Pool Limits")]
        [Tooltip("Максимум объектов одного типа в пуле. Остальное будет уничтожено.")]
        public int maxPerPrefab = 200;   // можешь увеличить до 400–600 для деревьев и мелочи
        public int maxTotalObjects = 5000; // глобальный крышняк

        private int _totalPooled = 0;

        private Transform _poolRoot;

        // prefabIndex → inactive objects
        private readonly Dictionary<int, Stack<PoolObject>> _pool =
            new Dictionary<int, Stack<PoolObject>>();

        private readonly Dictionary<int, Transform> _containers =
            new Dictionary<int, Transform>();


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _poolRoot = new GameObject("POOL_ROOT").transform;
            _poolRoot.SetParent(transform);
        }


        // =====================================================================
        // GET
        // =====================================================================
        public PoolObject Get(int prefabIndex, GameObject prefab)
        {
            PoolObject obj;

            if (_pool.TryGetValue(prefabIndex, out var stack) && stack.Count > 0)
            {
                obj = stack.Pop();
                obj.gameObject.SetActive(true);
            }
            else
            {
                // Создаём новый объект (НО теперь без утечек!)
                var go = Instantiate(prefab);

                obj = go.GetComponent<PoolObject>() ?? go.AddComponent<PoolObject>();
                obj.Pool = this;

                if (obj.meta == null)
                    obj.meta = go.AddComponent<PoolMeta>();

                obj.meta.prefabIndex = prefabIndex;
            }

            return obj;
        }


        // =====================================================================
        // RELEASE
        // =====================================================================
        public void Release(PoolObject obj)
        {
            if (obj == null)
                return;

            if (obj.meta == null)
                obj.meta = obj.gameObject.AddComponent<PoolMeta>();

            int index = obj.meta.prefabIndex;

            // Глобальный лимит: если уже много — просто уничтожаем
            if (_totalPooled >= maxTotalObjects)
            {
                Destroy(obj.gameObject);
                return;
            }

            var container = GetOrCreateContainer(index);

            if (_pool.TryGetValue(index, out var stack) && stack.Count >= maxPerPrefab)
            {
                Destroy(obj.gameObject);
                return;
            }

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(container, false);

            if (!_pool.TryGetValue(index, out stack))
            {
                stack = new Stack<PoolObject>();
                _pool[index] = stack;
            }

            stack.Push(obj);
            _totalPooled = ComputeTotalPooled(); // или вести счётчик аккуратно
        }

        private int ComputeTotalPooled()
        {
            int sum = 0;
            foreach (var kv in _pool)
                sum += kv.Value.Count;
            return sum;
        }



        // =====================================================================
        // CLEAR ALL POOLS (на случай смены локации / мира)
        // =====================================================================
        public void ClearAll()
        {
            foreach (var stack in _pool.Values)
            {
                foreach (var obj in stack)
                {
                    if (obj != null)
                        Destroy(obj.gameObject);
                }
            }

            _pool.Clear();

            foreach (var c in _containers.Values)
                if (c != null)
                    Destroy(c.gameObject);

            _containers.Clear();
        }


        // =====================================================================
        // INTERNAL
        // =====================================================================
        private Transform GetOrCreateContainer(int prefabIndex)
        {
            if (_containers.TryGetValue(prefabIndex, out var c))
                return c;

            var go = new GameObject($"Pool_{prefabIndex}");
            go.transform.SetParent(_poolRoot);
            _containers[prefabIndex] = go.transform;

            return go.transform;
        }

        public Dictionary<int, int> Debug_GetPoolCounts()
        {
            var dict = new Dictionary<int, int>();
            foreach (var kv in _pool)
                dict[kv.Key] = kv.Value.Count;

            return dict;
        }
    }
}
