using UnityEngine;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

namespace Features.Biomes.Runtime.Visual
{
    public class AdvancedWaterPlane : MonoBehaviour
    {
        [Header("World")]
        public WorldConfig world;

        [Header("Follow Target (player or camera)")]
        public Transform followTarget;

        [Header("Renderer")]
        public MeshRenderer waterRenderer;

        private void Reset()
        {
            waterRenderer = GetComponent<MeshRenderer>();
        }

        private void LateUpdate()
        {
            if (world == null)
                return;

            Vector3 pos = followTarget != null ? followTarget.position : Vector3.zero;

            float2 wp = new float2(pos.x, pos.z);
            var blend = world.GetBiomeBlend(wp);
            BiomeConfig biome = blend.biome;
            if (biome == null || !biome.useWater)
                return;

            Vector3 p = transform.position;
            p.y = biome.seaLevel;
            transform.position = p;

            if (waterRenderer == null)
                return;

            Material mat =
                biome.waterType == WaterType.Swamp ? biome.swampWaterMaterial :
                biome.waterType == WaterType.Lake  ? biome.lakeWaterMaterial :
                biome.waterMaterial != null        ? biome.waterMaterial :
                                                     biome.oceanWaterMaterial;

            if (mat != null)
                waterRenderer.sharedMaterial = mat;
        }
    }
}
