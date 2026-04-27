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

        [Header("Loading Screen")]
        public Sprite loadingBackground;

        [Header("Safe Spawn Zone")]
        public float safeSpawnFlatRadius = 50f;
        public float safeSpawnBlendRadius = 65f;
        public float safeSpawnHeightOffset = 5f;

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

            for (int i = 0; i < biomes.Length; i++)
            {
                var layer = biomes[i];
                if (layer.config == null)
                    continue;

                Vector2 seedOffset = GetSeedOffset(i);

                float noise = Mathf.PerlinNoise(
                    pos.x * layer.scale + layer.offset.x + seedOffset.x,
                    pos.z * layer.scale + layer.offset.y + seedOffset.y
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

            if (biomes == null || biomes.Length == 0)
                return new[] { new BiomeBlendResult(null, 0f) };

            for (int i = 0; i < biomes.Length; i++)
            {
                var layer = biomes[i];
                if (layer.config == null)
                    continue;

                Vector2 seedOffset = GetSeedOffset(i);

                float noise = Mathf.PerlinNoise(
                    worldPos.x * layer.scale + layer.offset.x + seedOffset.x,
                    worldPos.z * layer.scale + layer.offset.y + seedOffset.y
                );

                float maskW = layer.mask != null ? layer.mask.GetWeight(worldPos) : 1f;
                float w = noise * layer.weight * maskW;

                if (w > 0.0001f)
                    list.Add(new BiomeBlendResult(layer.config, w));
            }

            if (list.Count == 0)
                return new[] { new BiomeBlendResult(null, 0f) };

            float sum = 0f;
            foreach (var b in list)
                sum += b.weight;

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
            float terrainHeight = GetTerrainHeight(pos);
            float safeFactor = GetSafeSpawnFactor(pos);
            if (safeFactor >= 1f)
                return terrainHeight;

            var biome = GetBiomeBlend(GetSafeSpawnCenter()).biome;
            float seaLevel = biome != null ? biome.seaLevel : 0f;
            float safeHeight = seaLevel + safeSpawnHeightOffset;

            return math.lerp(safeHeight, terrainHeight, safeFactor);
        }

        public float GetSafeSpawnFactor(float2 pos)
        {
            float flatRadius = Mathf.Max(0f, safeSpawnFlatRadius);
            float blendRadius = Mathf.Max(flatRadius, safeSpawnBlendRadius);
            float dist = math.distance(pos, GetSafeSpawnCenter());

            if (dist < flatRadius)
                return 0f;

            if (dist >= blendRadius || blendRadius <= flatRadius)
                return 1f;

            float t = (dist - flatRadius) / (blendRadius - flatRadius);
            return t * t * (3f - 2f * t);
        }

        public float2 GetSafeSpawnCenter()
        {
            return new float2(chunkSize * 0.5f, chunkSize * 0.5f);
        }

        private float GetTerrainHeight(float2 pos)
        {
            var blends = GetBiomeBlend(new Vector3(pos.x, 0, pos.y));
            if (blends == null || blends.Length == 0)
                return 0f;

            float sum = 0f;
            float wsum = 0f;

            foreach (var b in blends)
            {
                if (b.biome == null || b.weight <= 0f)
                    continue;

                float h = BiomeHeightUtility.GetHeight(b.biome, pos.x, pos.y, seed);
                sum += h * b.weight;
                wsum += b.weight;
            }

            return wsum > 0f ? sum / wsum : 0f;
        }

        public TerrainJobData GetJobData()
        {
            return new TerrainJobData
            {
                chunkSize = chunkSize,
                biomes = biomes
            };
        }

        private Vector2 GetSeedOffset(int layerIndex)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)(layerIndex + 1) * 0x9E3779B9u;
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;

                uint x = h & 0xFFFFu;
                uint z = (h >> 16) & 0xFFFFu;

                return new Vector2(
                    x / 65535f * 10000f,
                    z / 65535f * 10000f
                );
            }
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
