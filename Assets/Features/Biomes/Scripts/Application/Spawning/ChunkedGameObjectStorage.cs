using System.Collections.Generic;
using UnityEngine;
using Features.Biomes.Application;
using Features.Pooling;

namespace Features.Biomes.Application.Spawning
{
    public static class ChunkedGameObjectStorage
    {
        public static int chunkSize;

        // живые объекты по чанкам
        private static readonly Dictionary<Vector2Int, List<GameObject>> storage =
            new Dictionary<Vector2Int, List<GameObject>>();

        // Reusable List для UNLOAD (чтобы избежать foreach-to-modify)
        private static readonly List<GameObject> unloadBuffer = new();


        // =========================================================
        // REGISTRATION
        // =========================================================
        public static void Register(Vector2Int coord, GameObject go)
        {
            if (!storage.TryGetValue(coord, out var list))
            {
                list = new List<GameObject>(64); // заранее резервируем место
                storage[coord] = list;
            }

            list.Add(go);
        }


        // =========================================================
        // UNLOAD (zero allocations)
        // =========================================================
        public static void Unload(Vector2Int coord)
        {
            if (!storage.TryGetValue(coord, out var list))
                return;

            unloadBuffer.Clear();
            unloadBuffer.AddRange(list);

            // unload objects
            foreach (var go in unloadBuffer)
            {
                if (go == null)
                    continue;

                // если объект из пула — возвращаем
                if (go.TryGetComponent<PoolObject>(out var pooled))
                {
                    pooled.ReturnToPool();
                }
                else
                {
                    Object.Destroy(go);
                }
            }

            // удаляем запись
            storage.Remove(coord);
        }


        // =========================================================
        // CLEAR-ALL (например, при смене мира)
        // =========================================================
        public static void ClearAll()
        {
            foreach (var kv in storage)
            {
                foreach (var go in kv.Value)
                {
                    if (go != null)
                        Object.Destroy(go);
                }
            }

            storage.Clear();
        }


        // =========================================================
        // ACTIVE CHUNKS → fills EXISTING LIST
        // =========================================================
        /// <summary>
        /// Заполняет готовый список runtime-данных координатами нужных чанков.
        /// НИЧЕГО НЕ АЛЛОЦИРУЕТ.
        /// </summary>
        public static void FillActiveChunks(List<Vector2Int> coords, List<ChunkRuntimeData> outList)
        {
            outList.Clear();
            int count = coords.Count;

            // гарантируем нужную вместимость (no new allocations after first frame)
            if (outList.Capacity < count)
                outList.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                outList.Add(new ChunkRuntimeData(coords[i]));
            }
        }


        // =========================================================
        // OPTIONAL: GET RAW OBJECTS FOR CHUNK (LOD/AI/DEBUG)
        // =========================================================
        public static List<GameObject> GetObjects(Vector2Int coord)
        {
            return storage.TryGetValue(coord, out var list) ? list : null;
        }
    }
}
