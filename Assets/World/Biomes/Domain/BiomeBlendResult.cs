using Biomes.Data;

namespace Biomes.Domain
{
    [System.Serializable]
    public struct BiomeBlendResult
    {
        public BiomeConfig biome;
        public float weight;

        public BiomeBlendResult(BiomeConfig biome, float weight)
        {
            this.biome = biome;
            this.weight = weight;
        }
    }
}
