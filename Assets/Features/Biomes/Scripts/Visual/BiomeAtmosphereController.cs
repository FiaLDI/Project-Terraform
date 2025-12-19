using UnityEngine;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;
using Features.Player.UnityIntegration;

namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(210)]
    public class BiomeAtmosphereController : MonoBehaviour
    {
        [Header("Weather Root")]
        public Transform weatherRoot;
        public Transform rainContainer;
        public Transform dustContainer;
        public Transform firefliesContainer;

        private BiomeConfig currentBiome;

        private void LateUpdate()
        {
            if (RuntimeWorldGenerator.World == null)
                return;

            var registry = PlayerRegistry.Instance;
            if (registry == null || registry.LocalPlayer == null)
                return;

            Vector3 pos = registry.LocalPlayer.transform.position;

            BiomeConfig biome = RuntimeWorldGenerator.World.GetBiomeAtWorldPos(pos);
            if (biome == null || biome == currentBiome)
                return;

            currentBiome = biome;
            ApplyBiome(biome, pos);
        }

        private void ApplyBiome(BiomeConfig biome, Vector3 playerPos)
        {
            // -----------------------------
            // SKYBOX
            // -----------------------------
            if (biome.skyboxMaterial != null)
            {
                RenderSettings.skybox = biome.skyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }

            // -----------------------------
            // WEATHER ROOT FOLLOW
            // -----------------------------
            if (weatherRoot != null)
            {
                weatherRoot.position = playerPos;
            }

            // -----------------------------
            // WEATHER PREFABS
            // -----------------------------
            ToggleWeather(rainContainer, biome.rainPrefab);
            ToggleWeather(dustContainer, biome.dustPrefab);
            ToggleWeather(firefliesContainer, biome.firefliesPrefab);
        }

        private void ToggleWeather(Transform container, GameObject prefab)
        {
            if (container == null)
                return;

            if (prefab == null)
            {
                if (container.childCount > 0)
                {
                    for (int i = container.childCount - 1; i >= 0; i--)
                        Destroy(container.GetChild(i).gameObject);
                }
                return;
            }

            if (container.childCount == 0)
                Instantiate(prefab, container);
        }
    }
}
