using System.Collections.Generic;
using UnityEngine;

namespace Features.Pooling
{
    /// <summary>
    /// Улучшенный SmartPool:
    /// - единый синглтон;
    /// - создаёт контейнеры для каждого prefabIndex;
    /// - возвращённые объекты хранятся под POOL_ROOT;
    /// - не засоряет иерархию;
    /// - безопасно работает с PoolObject/PoolMeta.
    /// </summary>
    public class SmartPool : MonoBehaviour
    {
        public static SmartPool Instance;

        /// <summary>
        /// Родитель всех контейнеров пулов.
        /// </summary>
        private Transform _poolRoot;

        /// <summary>
        /// Свободные объекты по prefabIndex.
        /// </summary>
        private readonly Dictionary<int, Stack<PoolObject>> _pool =
            new Dictionary<int, Stack<PoolObject>>();

        /// <summary>
        /// Контейнеры по prefabIndex.
        /// </summary>
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

        /// <summary>
        /// Получить объект из пула или создать новый.
        /// prefabIndex — ключ пула, prefab — образец для Instantiate.
        /// </summary>
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
                var go = Object.Instantiate(prefab);
                obj = go.GetComponent<PoolObject>();
                if (obj == null)
                    obj = go.AddComponent<PoolObject>();

                // Привязываем к пулу
                obj.Pool = this;

                // Гарантируем наличие meta
                if (obj.meta == null)
                    obj.meta = go.AddComponent<PoolMeta>();

                // prefabIndex выставится снаружи (RuntimeSpawnerSystem)
            }

            return obj;
        }

        /// <summary>
        /// Вернуть объект в пул.
        /// </summary>
        public void Release(PoolObject obj)
        {
            if (obj == null)
                return;

            if (obj.meta == null)
                obj.meta = obj.gameObject.GetComponent<PoolMeta>() ?? obj.gameObject.AddComponent<PoolMeta>();

            int index = obj.meta.prefabIndex;

            obj.gameObject.SetActive(false);

            var container = GetOrCreateContainer(index);
            obj.transform.SetParent(container, false);

            if (!_pool.TryGetValue(index, out var stack))
            {
                stack = new Stack<PoolObject>();
                _pool[index] = stack;
            }

            stack.Push(obj);
        }

        /// <summary>
        /// Создать или получить контейнер для prefabIndex.
        /// </summary>
        private Transform GetOrCreateContainer(int prefabIndex)
        {
            if (_containers.TryGetValue(prefabIndex, out var c))
                return c;

            var go = new GameObject($"Pool_{prefabIndex}");
            go.transform.SetParent(_poolRoot);
            c = go.transform;
            _containers[prefabIndex] = c;
            return c;
        }
    }
}
