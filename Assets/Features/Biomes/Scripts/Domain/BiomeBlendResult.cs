using UnityEngine;

namespace Features.Biomes.Domain
{
    /// <summary>
    /// Результат смешивания биомов в точке мира.
    /// Возвращается WorldConfig.GetBiomeBlend(...)
    /// </summary>
    [System.Serializable]
    public struct BiomeBlendResult
    {
        public BiomeConfig biome;
        public float weight;     // 0..1

        public BiomeBlendResult(BiomeConfig biome, float weight)
        {
            this.biome = biome;
            this.weight = weight;
        }
    }
}
