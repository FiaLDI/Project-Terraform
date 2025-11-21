using UnityEngine;

public class BiomeWorldVisualController : MonoBehaviour
{
    public WorldConfig worldConfig;
    public Transform player;

    private BiomeConfig currentBiome;

    private void Update()
    {
        if (worldConfig == null || player == null) return;

        BiomeConfig biome = worldConfig.GetBiomeAtWorldPos(player.position);
        if (biome == null || biome == currentBiome) return;

        currentBiome = biome;
        ApplyBiomeVisuals(biome);
    }

    private void ApplyBiomeVisuals(BiomeConfig biome)
    {
        if (biome.skyboxMaterial != null)
        {
            RenderSettings.skybox = biome.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        if (biome.enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = biome.fogMode;
            RenderSettings.fogColor = biome.fogColor;

            if (biome.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = biome.fogLinearStart;
                RenderSettings.fogEndDistance = biome.fogLinearEnd;
            }
            else
            {
                RenderSettings.fogDensity = biome.fogDensity;
            }
        }
        else
        {
            RenderSettings.fog = false;
        }
    }
}
