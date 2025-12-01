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

            // biome blend по позиции
            var blends = world.GetBiomeBlend(new Vector3(pos.x, 0f, pos.z));
            if (blends == null || blends.Length == 0)
                return;

            // выбираем биом по максимальному весу
            BiomeConfig biome = null;
            float bestW = 0f;

            foreach (var b in blends)
            {
                if (b.biome == null) continue;
                if (b.weight > bestW)
                {
                    bestW = b.weight;
                    biome = b.biome;
                }
            }

            if (biome == null || !biome.useWater)
                return;

            // -----------------------
            // 1) Уровень воды (seaLevel)
            // -----------------------
            Vector3 p = transform.position;
            p.y = biome.seaLevel;
            transform.position = p;

            // -----------------------
            // 2) Материал воды
            // -----------------------
            if (waterRenderer == null)
                return;

            Material chosenMat = null;

            switch (biome.waterType)
            {
                case WaterType.Swamp:
                    chosenMat = biome.swampWaterMaterial;
                    break;

                case WaterType.Lake:
                    chosenMat = biome.lakeWaterMaterial;
                    break;

                default: // Ocean / Generic
                    chosenMat = biome.waterMaterial != null
                        ? biome.waterMaterial
                        : biome.oceanWaterMaterial;
                    break;
            }

            if (chosenMat != null)
            {
                // sharedMaterial — чтобы не клонировать материал на каждом кадре
                waterRenderer.sharedMaterial = chosenMat;
            }
        }
    }
}
