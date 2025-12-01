using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace Features.Biomes.Domain
{
    [CreateAssetMenu(menuName = "Game/World Config")]
    public class WorldConfig : ScriptableObject
    {
        [Header("Chunk Settings")]
        public int chunkSize = 64;

        [Header("Biome Layers")]
        public BiomeLayer[] biomes;

        // ===== BIOME RESOLUTION =====

        public BiomeConfig GetBiomeAtChunk(Vector2Int chunk)
        {
            Vector3 worldPos = new Vector3(
                chunk.x * chunkSize,
                0,
                chunk.y * chunkSize
            );

            return GetBiomeAtWorldPos(worldPos);
        }

        public BiomeConfig GetBiomeAtWorldPos(Vector3 pos)
        {
            if (biomes == null || biomes.Length == 0)
                return null;

            float best = float.MinValue;
            BiomeConfig result = biomes[0].config;

            foreach (var layer in biomes)
            {
                if (layer.config == null)
                    continue;

                float noise =
                    Mathf.PerlinNoise(
                        pos.x * layer.scale + layer.offset.x,
                        pos.z * layer.scale + layer.offset.y
                    );

                float v = noise * layer.weight;

                if (v > best)
                {
                    best = v;
                    result = layer.config;
                }
            }

            return result;
        }

        public (BiomeConfig biome, float blend) GetDominantBiome(Vector2Int chunk)
        {
            return (GetBiomeAtChunk(chunk), 1f);
        }

        public BiomeBlendResult[] GetBiomeBlend(Vector3 worldPos)
        {
            // это твой старый код биомов — я оставляю КАК ЕСТЬ:
            // (примерная логика)
            
            List<BiomeBlendResult> list = new List<BiomeBlendResult>(4);

            foreach (var layer in biomes)
            {
                float w = layer.mask.GetWeight(worldPos);
                if (w > 0f)
                    list.Add(new BiomeBlendResult(layer.config, w));
            }

            if (list.Count == 0)
                return new[] { new BiomeBlendResult(default, 0f) };

            return list.ToArray();
        }

        public BiomeBlendResult GetBiomeBlend(float2 worldXZ)
        {
            Vector3 wp = new Vector3(worldXZ.x, 0, worldXZ.y);

            var arr = GetBiomeBlend(wp);
            if (arr == null || arr.Length == 0)
                return default;

            BiomeBlendResult best = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].weight > best.weight)
                    best = arr[i];
            }

            return best;
        }


        public float GetHeight(float2 pos)
        {
            var blends = GetBiomeBlend(new Vector3(pos.x, 0, pos.y));
            if (blends == null || blends.Length == 0) return 0f;

            float sum = 0f, wsum = 0f;
            foreach (var b in blends)
            {
                if (b.biome == null || b.weight <= 0f) continue;
                float h = BiomeHeightUtility.GetHeight(b.biome, pos.x, pos.y);
                sum  += h * b.weight;
                wsum += b.weight;
            }
            return wsum > 0f ? sum / wsum : 0f;
        }


        // ===== JOB DATA SUPPORT =====

        public TerrainJobData GetJobData()
        {
            return new TerrainJobData
            {
                chunkSize = this.chunkSize,
                biomes = this.biomes
            };
        }
    }

    [System.Serializable]
    public class BiomeLayer
    {
        public BiomeConfig config;
        public float weight = 1f;
        public float scale = 0.001f;
        public Vector2 offset;

        public BiomeMask mask = new BiomeMask();
    }

    public struct TerrainJobData
    {
        public int chunkSize;
        public BiomeLayer[] biomes;
    }
}
