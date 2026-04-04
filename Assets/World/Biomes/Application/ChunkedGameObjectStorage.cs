using System.Collections.Generic;
using UnityEngine;

namespace Biomes.Application
{
    public static class ChunkedGameObjectStorage
    {
        public static int chunkSize;

        private static readonly Dictionary<Vector2Int, List<GameObject>> storage =
            new Dictionary<Vector2Int, List<GameObject>>();

        private static readonly List<GameObject> unloadBuffer = new();

        public static void Register(Vector2Int coord, GameObject go)
        {
            if (!storage.TryGetValue(coord, out var list))
            {
                list = new List<GameObject>(64);
                storage[coord] = list;
            }

            list.Add(go);
        }

        public static void Unload(Vector2Int coord)
        {
            if (!storage.TryGetValue(coord, out var list))
                return;

            unloadBuffer.Clear();
            unloadBuffer.AddRange(list);

            foreach (var go in unloadBuffer)
            {
                if (go == null)
                    continue;

                Object.Destroy(go);
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

        public static void FillActiveChunks(List<Vector2Int> coords, List<ChunkRuntimeData> outList)
        {
            outList.Clear();
            int count = coords.Count;

            if (outList.Capacity < count)
                outList.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                outList.Add(new ChunkRuntimeData(coords[i]));
            }
        }

        public static List<GameObject> GetObjects(Vector2Int coord)
        {
            return storage.TryGetValue(coord, out var list) ? list : null;
        }
    }
}
