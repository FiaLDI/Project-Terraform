using UnityEngine;
using Features.Player.UnityIntegration;
using Biomes.Data;

namespace Biomes.UnityIntegration
{
    [DefaultExecutionOrder(210)]
    public class BiomeAtmosphereController : MonoBehaviour
    {
        [Header("Weather Root")]
        public Transform weatherRoot;
        public Transform rainContainer;
        public Transform dustContainer;
        public Transform firefliesContainer;

        private WorldConfig currentWorld;
        private BiomeConfig currentBiome;
        private Material defaultSkybox;

        private void Awake()
        {
            defaultSkybox = RenderSettings.skybox;
        }

        private void OnDisable()
        {
            RestoreDefaults();
        }

        private void LateUpdate()
        {
            var activeWorld = RuntimeWorldGenerator.World;
            if (activeWorld != currentWorld)
            {
                currentWorld = activeWorld;
                currentBiome = null;

                if (currentWorld == null)
                {
                    RestoreDefaults();
                    return;
                }

                ClearAllWeather();
            }

            if (currentWorld == null)
                return;

            var registry = PlayerRegistry.Instance;
            if (registry == null || registry.LocalPlayer == null)
                return;

            Vector3 pos = registry.LocalPlayer.transform.position;
            if (weatherRoot != null)
                weatherRoot.position = pos;

            BiomeConfig biome = currentWorld.GetBiomeAtWorldPos(pos);
            if (biome == null)
            {
                if (currentBiome != null)
                {
                    currentBiome = null;
                    RestoreDefaults();
                }
                return;
            }

            if (biome == currentBiome)
                return;

            currentBiome = biome;
            ApplyBiome(biome, pos);
        }

        private void ApplyBiome(BiomeConfig biome, Vector3 playerPos)
        {
            RenderSettings.skybox = biome.skyboxMaterial != null
                ? biome.skyboxMaterial
                : defaultSkybox;
            DynamicGI.UpdateEnvironment();

            if (weatherRoot != null)
            {
                weatherRoot.position = playerPos;
            }

            ToggleWeather(rainContainer, biome.rainPrefab);
            ToggleWeather(dustContainer, biome.dustPrefab);
            ToggleWeather(firefliesContainer, biome.firefliesPrefab);
        }

        private void ToggleWeather(Transform container, GameObject prefab)
        {
            if (container == null)
                return;

            GameObject existing = container.childCount > 0
                ? container.GetChild(0).gameObject
                : null;

            if (prefab == null)
            {
                ClearContainer(container);
                return;
            }

            if (existing != null && existing.name.StartsWith(prefab.name))
                return;

            ClearContainer(container);
            if (container.childCount == 0)
                Instantiate(prefab, container);
        }

        private void ClearAllWeather()
        {
            ClearContainer(rainContainer);
            ClearContainer(dustContainer);
            ClearContainer(firefliesContainer);
        }

        private void ClearContainer(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private void RestoreDefaults()
        {
            RenderSettings.skybox = defaultSkybox;
            DynamicGI.UpdateEnvironment();
            ClearAllWeather();
        }
    }
}
