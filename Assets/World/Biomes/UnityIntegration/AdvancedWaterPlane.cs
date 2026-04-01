using Biomes.Data;
using Features.Player.UnityIntegration;
using UnityEngine;

namespace Biomes.UnityIntegration
{
    public class AdvancedWaterPlane : MonoBehaviour
    {
        public Transform water;
        [Header("Renderer")]
        public MeshRenderer waterRenderer;

        private Transform followTarget;
        private WorldConfig world;

        private void Reset()
        {
            waterRenderer = water.GetComponent<MeshRenderer>();
        }

        private void OnEnable()
        {
            PlayerRegistry.SubscribeLocalPlayerReady(OnPlayerReady);
        }

        private void OnDisable()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnPlayerReady);
        }

        private void OnPlayerReady(PlayerRegistry registry)
        {
            if (registry.LocalPlayer != null)
            {
                followTarget = registry.LocalPlayer.transform;
                Debug.Log("[Water] Follow target assigned");
            }
        }

        private void LateUpdate()
        {
            if (world == null)
                world = RuntimeWorldGenerator.World;

            if (world == null || followTarget == null)
                return;

            Vector3 pos = followTarget.position;

            var blends = world.GetBiomeBlend(new Vector3(pos.x, 0f, pos.z));
            if (blends == null || blends.Length == 0)
                return;

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

            water.position = new Vector3(
                followTarget.position.x,
                biome.seaLevel,
                followTarget.position.z
            );

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

                default:
                    chosenMat = biome.waterMaterial != null
                        ? biome.waterMaterial
                        : biome.oceanWaterMaterial;
                    break;
            }

            if (chosenMat != null)
                waterRenderer.sharedMaterial = chosenMat;
        }
    }
}
