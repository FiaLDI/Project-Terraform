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
        private Material defaultMaterial;
        private bool defaultRendererEnabled;

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
            RestoreDefaults();
        }

        private void OnPlayerReady(PlayerRegistry registry)
        {
            if (registry.LocalPlayer != null)
            {
                followTarget = registry.LocalPlayer.transform;
                Debug.Log("[Water] Follow target assigned");
            }
        }

        private void Start()
        {
            if (waterRenderer == null && water != null)
                waterRenderer = water.GetComponent<MeshRenderer>();

            if (waterRenderer != null)
            {
                defaultMaterial = waterRenderer.sharedMaterial;
                defaultRendererEnabled = waterRenderer.enabled;
            }
        }

        private void LateUpdate()
        {
            var activeWorld = RuntimeWorldGenerator.World;
            if (activeWorld != world)
                BindWorld(activeWorld);

            if (world == null || followTarget == null)
            {
                DisableWater();
                return;
            }

            Vector3 pos = followTarget.position;

            var blends = world.GetBiomeBlend(new Vector3(pos.x, 0f, pos.z));
            if (blends == null || blends.Length == 0)
            {
                DisableWater();
                return;
            }

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
            {
                DisableWater();
                return;
            }

            EnableWater();

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

            waterRenderer.sharedMaterial = chosenMat != null ? chosenMat : defaultMaterial;
        }

        private void BindWorld(WorldConfig activeWorld)
        {
            world = activeWorld;
            if (world == null)
                DisableWater();
        }

        private void EnableWater()
        {
            if (water != null && !water.gameObject.activeSelf)
                water.gameObject.SetActive(true);

            if (waterRenderer != null)
                waterRenderer.enabled = true;
        }

        private void DisableWater()
        {
            if (waterRenderer != null)
            {
                waterRenderer.enabled = false;
                waterRenderer.sharedMaterial = defaultMaterial;
            }

            if (water != null && water.gameObject.activeSelf)
                water.gameObject.SetActive(false);
        }

        private void RestoreDefaults()
        {
            if (water != null)
                water.gameObject.SetActive(defaultRendererEnabled);

            if (waterRenderer != null)
            {
                waterRenderer.enabled = defaultRendererEnabled;
                waterRenderer.sharedMaterial = defaultMaterial;
            }
        }
    }
}
