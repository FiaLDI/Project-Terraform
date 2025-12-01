using UnityEngine;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(210)]
    public class BiomeAtmosphereController : MonoBehaviour
    {
        [Header("Targets")]
        public Transform player;

        [Header("Weather Root")]
        public Transform weatherRoot;
        public Transform rainContainer;
        public Transform dustContainer;
        public Transform firefliesContainer;

        private BiomeConfig currentBiome;

        private void Awake()
        {
            if (player == null && RuntimeWorldGenerator.PlayerInstance != null)
                player = RuntimeWorldGenerator.PlayerInstance.transform;
        }


        private void LateUpdate()
        {
            if (RuntimeWorldGenerator.World == null) return;
            if (RuntimeWorldGenerator.PlayerInstance == null) return;

            Vector3 pos = RuntimeWorldGenerator.PlayerInstance.transform.position;

            BiomeConfig biome = RuntimeWorldGenerator.World.GetBiomeAtWorldPos(pos);
            if (biome == null || biome == currentBiome)
                return;

            currentBiome = biome;
            ApplyBiome(biome);
        }

        private void ApplyBiome(BiomeConfig biome)
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
            // WEATHER
            // -----------------------------
            if (weatherRoot != null)
            {
                weatherRoot.position = RuntimeWorldGenerator.PlayerInstance.transform.position;
            }

            ToggleWeather(rainContainer, biome.rainPrefab);
            ToggleWeather(dustContainer, biome.dustPrefab);
            ToggleWeather(firefliesContainer, biome.firefliesPrefab);
        }

        private void ToggleWeather(Transform container, GameObject prefab)
        {
            if (container == null) return;

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
