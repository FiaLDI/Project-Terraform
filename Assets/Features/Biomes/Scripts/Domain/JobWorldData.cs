using Unity.Mathematics;

namespace Features.Biomes.Domain
{
    public struct JobWorldData
    {
        public float baseHeight;
        public float noiseScale;

        public float SampleHeight(float x, float z)
        {
            float n = noise.snoise(new float2(x * noiseScale, z * noiseScale));
            return baseHeight * (n * 0.5f + 0.5f); 
        }
    }
}
