using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Biomes.Domain;
using Biomes.Utility;

namespace Biomes.Data
{
    [CreateAssetMenu(menuName = "Game/World Config")]
    public class WorldConfig : ScriptableObject
    {
        [Header("World Generation")]
        public int seed = 12345;
        
        [Header("Chunk Settings")]
        public int chunkSize = 64;

        [Header("Biome Layers")]
        public BiomeLayer[] biomes;

        [Header("Global Ground Material")]
        public Material worldGroundMaterial;

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
            var list = new List<BiomeBlendResult>(4);

            foreach (var layer in biomes)
            {
                if (layer.config == null)
                    continue;

                float noise = Mathf.PerlinNoise(
                    worldPos.x * layer.scale + layer.offset.x,
                    worldPos.z * layer.scale + layer.offset.y
                );

                float maskW = layer.mask != null ? layer.mask.GetWeight(worldPos) : 1f;

                float w = noise * layer.weight * maskW;

                if (w > 0.0001f)
                    list.Add(new BiomeBlendResult(layer.config, w));
            }

            if (list.Count == 0)
                return new[] { new BiomeBlendResult(null, 0f) };

            float sum = 0f;
            foreach (var b in list) sum += b.weight;

            if (sum > 0f)
            {
                for (int i = 0; i < list.Count; i++)
                    list[i] = new BiomeBlendResult(list[i].biome, list[i].weight / sum);
            }

            return list.ToArray();
        }

        public Vector2Int WorldToChunk(Vector3 pos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / chunkSize),
                Mathf.FloorToInt(pos.z / chunkSize)
            );
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
            // ================= BASE TERRAIN =================
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

            float terrainHeight = wsum > 0f ? sum / wsum : 0f;

            // ================= SAFE SPAWN PLATFORM =================

            float2 center = float2.zero;
            float dist = math.distance(pos, center);

            float flatRadius = 50f;     // радиус плоской зоны
            float blendRadius = 65f;    // радиус перехода

            var biome = GetBiomeBlend(float2.zero).biome;

            float seaLevel = biome != null ? biome.seaLevel : 0f;

            float baseHeight = seaLevel + 5f; // ВСЕГДА выше воды

            // ================= LOGIC =================

            if (dist < flatRadius)
            {
                return baseHeight;
            }

            if (dist < blendRadius)
            {
                float t = (dist - flatRadius) / (blendRadius - flatRadius);

                // smoothstep
                float smooth = t * t * (3f - 2f * t);

                return math.lerp(baseHeight, terrainHeight, smooth);
            }

            return terrainHeight;
        }

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
