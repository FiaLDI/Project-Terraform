using System.Collections.Generic;
using UnityEngine;
using Features.Biomes.Application;
using Features.Pooling;

namespace Features.Biomes.Application.Spawning
{
    /// <summary>
    /// Хранилище всех заспавненных GameObject по чанкам +
    /// генерация списка ChunkRuntimeData для LOD-системы.
    /// Сейчас ChunkRuntimeData заполняется только координатой чанка,
    /// Buckets остаются пустыми (их можно будет заполнить позже,
    /// когда будет готов пайплайн инстансинга).
    /// </summary>
    public static class ChunkedGameObjectStorage
    {
        // Размер чанка, задаётся из ChunkManager
        public static int chunkSize;

        // Все живые объекты по чанкам
        private static readonly Dictionary<Vector2Int, List<GameObject>> storage =
            new Dictionary<Vector2Int, List<GameObject>>();

        // ---------------------------------
        // REGISTRATION
        // ---------------------------------
        public static void Register(Vector2Int coord, GameObject go)
        {
            if (!storage.TryGetValue(coord, out var list))
            {
                list = new List<GameObject>();
                storage[coord] = list;
            }

            list.Add(go);
        }

        // ---------------------------------
        // UNLOAD
        // ---------------------------------
        public static void Unload(Vector2Int coord)
        {
            if (!storage.TryGetValue(coord, out var list))
                return;

            foreach (var go in list)
            {
                if (go == null)
                    continue;

                var pooled = go.GetComponent<PoolObject>();
                if (pooled != null)
                {
                    // Вернуть в пул
                    pooled.ReturnToPool();
                }
                else
                {
                    Object.Destroy(go);
                }
            }

            storage.Remove(coord);
        }

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

        // ---------------------------------
        // ACTIVE RUNTIME DATA FOR LOD
        // ---------------------------------
        public static List<ChunkRuntimeData> GetActiveChunks(List<Vector2Int> coords)
        {
            var result = new List<ChunkRuntimeData>(coords.Count);

            foreach (var coord in coords)
            {
                // Используем уже существующий класс
                // Features.Biomes.Application.ChunkRuntimeData
                var data = new ChunkRuntimeData(coord);

                // TODO: если нужно, здесь можно будет заполнить Buckets
                // из storage[coord], создавая InstanceData для каждого объекта.

                result.Add(data);
            }

            return result;
        }
    }
}
