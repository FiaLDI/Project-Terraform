using System.Collections.Generic;
using UnityEngine;

namespace Features.Biomes.Application
{
    public class ChunkRuntimeData
    {
        public Vector2Int coord;

        // prefabIndex → список инстансов этого типа
        public readonly Dictionary<int, List<InstanceData>> Buckets = new();

        public ChunkRuntimeData(Vector2Int c)
        {
            coord = c;
        }

        public void AddInstance(int prefabIndex, InstanceData inst)
        {
            if (!Buckets.ContainsKey(prefabIndex))
                Buckets[prefabIndex] = new List<InstanceData>();

            Buckets[prefabIndex].Add(inst);
        }
    }

    public struct InstanceData
    {
        public Vector3 position;
        public Vector3 normal;
        public float scale;
        public float random;
        public int biomeId;
    }
}
